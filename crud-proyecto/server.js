require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const mysql = require('mysql');

const app = express();
const port = 6000;

app.use(cors());
app.use(bodyParser.json({ limit: '5000mb' }));
app.use(bodyParser.urlencoded({ limit: '5000mb', extended: true }));

// Configuración de la base de datos
const db = mysql.createConnection({
    host: 'localhost',
    user: 'root',
    password: '',
    database: 'dbproyecto'
});

db.connect((err) => {
    if (err) {
        console.log('Error no está conectado a la base de datos');
        return;
    }
    console.log('Conectado a la base de datos');
});

const SECRET_TOKEN = process.env.SECRET_TOKEN;

// Middleware para verificar el Bearer Token
const authenticateToken = (req, res, next) => {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];

    console.log(`Received Token: ${token}`);
    console.log(`Expected Token: ${SECRET_TOKEN}`);

    if (!token || token !== SECRET_TOKEN) {
        return res.sendStatus(403); // Forbidden
    }

    next();
};

// Endpoint para crear usuario (no protegido)
app.post('/api/user', (req, res) => {
    const { nombre, correo, password, foto } = req.body;
    const consulta = 'INSERT INTO user (nombre, correo, password, foto) VALUES (?, ?, ?, ?)';

    db.query(consulta, [nombre, correo, password, foto], (err, result) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(result);
    });
});

// Endpoint para leer usuarios (protegido)
app.get('/api/user', authenticateToken, (req, res) => {
    const consulta = 'SELECT * FROM user';

    db.query(consulta, (err, resultado) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(resultado);
    });
});

// Endpoint para actualizar usuario (protegido)
app.put('/api/user/:id', authenticateToken, (req, res) => {
    const { id } = req.params;
    const { nombre, correo, password, foto } = req.body;

    const consulta = 'UPDATE user SET nombre = ?, correo = ?, password = ?, foto = ? WHERE id = ?';

    db.query(consulta, [nombre, correo, password, foto, id], (err, result) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(result);
    });
});

// Endpoint para eliminar usuario (protegido)
app.delete('/api/user/:id', authenticateToken, (req, res) => {
    const { id } = req.params;
    const consulta = 'DELETE FROM user WHERE id = ?';

    db.query(consulta, [id], (err, result) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(result);
    });
});

// Endpoint para agregar una nota con imágenes
app.post('/api/note', (req, res) => {
    const { userId, title, content, images } = req.body;

    // Insertar la nota en la base de datos
    const noteQuery = 'INSERT INTO notes (user_id, title, content, created_at) VALUES (?, ?, ?, NOW())';
    db.query(noteQuery, [userId, title, content], (err, result) => {
        if (err) {
            console.error('Error inserting note:', err);
            return res.status(500).json({ error: 'Error inserting note' });
        }

        const noteId = result.insertId;

        // Insertar las imágenes en la base de datos
        if (images && images.length > 0) {
            const imageQuery = 'INSERT INTO images (note_id, image) VALUES ?';
            const imageValues = images.map(image => [noteId, Buffer.from(image, 'base64')]);

            db.query(imageQuery, [imageValues], (err, result) => {
                if (err) {
                    console.error('Error inserting images:', err);
                    return res.status(500).json({ error: 'Error inserting images' });
                }

                res.status(200).json({ message: 'Note and images inserted successfully' });
            });
        } else {
            res.status(200).json({ message: 'Note inserted successfully' });
        }
    });
});

// Endpoint para leer notas y sus imágenes (protegido)
app.get('/api/note', authenticateToken, (req, res) => {
    const { userId } = req.query;
    const query = `
        SELECT n.id, n.title, n.content, n.created_at, i.id as image_id, i.image
        FROM notes n
        LEFT JOIN images i ON n.id = i.note_id
        WHERE n.user_id = ?
    `;

    db.query(query, [userId], (err, results) => {
        if (err) {
            res.status(500).send(err);
            return;
        }

        const notes = {};

        results.forEach(row => {
            if (!notes[row.id]) {
                notes[row.id] = {
                    id: row.id,
                    title: row.title,
                    content: row.content,
                    created_at: row.created_at,
                    images: []
                };
            }
            if (row.image_id) {
                notes[row.id].images.push(row.image.toString('base64'));
            }
        });

        res.status(200).send(Object.values(notes));
    });
});

// Endpoint para leer una nota específica y sus imágenes (protegido)
app.get('/api/note/:id', authenticateToken, (req, res) => {
    const { id } = req.params;

    const query = `
        SELECT n.id, n.title, n.content, n.created_at, i.id as image_id, i.image
        FROM notes n
        LEFT JOIN images i ON n.id = i.note_id
        WHERE n.id = ?
    `;

    db.query(query, [id], (err, results) => {
        if (err) {
            res.status(500).send(err);
            return;
        }

        if (results.length === 0) {
            res.status(404).json({ message: 'Note not found' });
            return;
        }

        const note = {
            id: results[0].id,
            title: results[0].title,
            content: results[0].content,
            created_at: results[0].created_at,
            images: []
        };

        results.forEach(row => {
            if (row.image_id) {
                note.images.push(row.image.toString('base64'));
            }
        });

        res.status(200).json(note);
    });
});

// Endpoint para actualizar una nota con imágenes (protegido)
app.put('/api/note/:id', authenticateToken, (req, res) => {
    const { id } = req.params;
    const { title, content, images } = req.body;

    // Actualizar la nota en la base de datos
    const noteQuery = 'UPDATE notes SET title = ?, content = ?, updated_at = NOW() WHERE id = ?';
    db.query(noteQuery, [title, content, id], (err, result) => {
        if (err) {
            console.error('Error updating note:', err);
            return res.status(500).json({ error: 'Error updating note' });
        }

        // Eliminar imágenes existentes para la nota
        const deleteImagesQuery = 'DELETE FROM images WHERE note_id = ?';
        db.query(deleteImagesQuery, [id], (err, result) => {
            if (err) {
                console.error('Error deleting images:', err);
                return res.status(500).json({ error: 'Error deleting images' });
            }

            // Insertar las nuevas imágenes en la base de datos
            if (images && images.length > 0) {
                const imageQuery = 'INSERT INTO images (note_id, image) VALUES ?';
                const imageValues = images.map(image => [id, Buffer.from(image, 'base64')]);

                db.query(imageQuery, [imageValues], (err, result) => {
                    if (err) {
                        console.error('Error inserting images:', err);
                        return res.status(500).json({ error: 'Error inserting images' });
                    }

                    res.status(200).json({ message: 'Note and images updated successfully' });
                });
            } else {
                res.status(200).json({ message: 'Note updated successfully' });
            }
        });
    });
});

// Endpoint para eliminar una nota y sus imágenes (protegido)
app.delete('/api/note/:id', authenticateToken, (req, res) => {
    const { id } = req.params;

    // Eliminar las imágenes de la nota
    const deleteImagesQuery = 'DELETE FROM images WHERE note_id = ?';
    db.query(deleteImagesQuery, [id], (err, result) => {
        if (err) {
            console.error('Error deleting images:', err);
            return res.status(500).json({ error: 'Error deleting images' });
        }

        // Eliminar la nota
        const deleteNoteQuery = 'DELETE FROM notes WHERE id = ?';
        db.query(deleteNoteQuery, [id], (err, result) => {
            if (err) {
                console.error('Error deleting note:', err);
                return res.status(500).json({ error: 'Error deleting note' });
            }

            res.status(200).json({ message: 'Note and images deleted successfully' });
        });
    });
});

app.listen(port, () => {
    console.log(`Servidor ejecutándose en http://192.168.1.6:${port}`);
});

