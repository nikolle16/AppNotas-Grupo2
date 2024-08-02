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

//USER
//Crear user
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

//Leer user
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

//Actualizar user
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

//Eliminar user
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

//NOTAS
//Agregar una nota con imágenes
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

//Leer notas y sus imágenes
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

//Leer una nota específica y sus imágenes
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

//Actualizar una nota con imágenes
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

//Eliminar una nota y sus imágenes
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

//AUDIO
//Agregar audio
app.post('/api/audios', (req, res) => {
    const { userId, title, audio, fecha } = req.body;

    const noteQuery = 'INSERT INTO audios (userId, title, audio, fecha) VALUES (?, ?, ?, ?)';
    db.query(noteQuery, [ userId, title, audio, fecha ], (err, result) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(result);
    });
});

//Leer audio
app.get('/api/audios', authenticateToken, (req, res) => {
    const { userId } = req.query;
    const query = `
        SELECT a.id, a.title, a.audio, a.fecha
        FROM audios a
        WHERE a.userId = ?
    `;

    db.query(query, [userId], (err, results) => {
        if (err) {
            res.status(500).send(err);
            return;
        }

        const audios = {};

        results.forEach(row => {
            if (!audios[row.id]) {
                audios[row.id] = {
                    id: row.id,
                    title: row.title,
                    audio: row.audio,
                    fecha: row.fecha
                };
            }
        });

        res.status(200).send(Object.values(audios));
    });
});

//Eliminar audio
app.delete('/api/audios/:id', authenticateToken, (req, res) => {
    const { id } = req.params;
    const consulta = 'DELETE FROM audios WHERE id = ?';

    db.query(consulta, [id], (err, result) => {
        if (err) {
            res.status(500).send(err);
            return;
        }
        res.status(200).send(result);
    });
});

app.listen(port, () => {
    console.log(`Servidor ejecutándose en http://192.168.0.3:${port}`);
});
