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

// Endpoint para leer notas y sus imágenes filtradas por user_id (protegido)
app.get('/api/note', authenticateToken, (req, res) => {
    const userId = req.query.userId;

    if (!userId) {
        return res.status(400).json({ error: 'userId is required' });
    }

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

app.listen(port, () => {
    console.log(`Servidor ejecutándose en http://192.168.1.6:${port}`);
});


