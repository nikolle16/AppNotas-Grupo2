require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const mysql = require('mysql');

const app = express();
const port = 6000;

app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ limit: '50mb', extended: true }));

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

app.listen(port, () => {
    console.log(`Servidor ejecutándose en http://192.168.0.7:${port}`);
});
