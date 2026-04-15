create database PETADOPCION;

CREATE TABLE Roles (
    id_rol INT IDENTITY PRIMARY KEY,
    nombre_rol VARCHAR(50) NOT NULL
);
INSERT INTO Roles (nombre_rol)
VALUES ('Administrador'), ('Usuario');


CREATE TABLE Usuarios (
    id_usuario INT IDENTITY PRIMARY KEY,
    id_rol INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    telefono VARCHAR(20),
    direccion VARCHAR(200),
    ciudad VARCHAR(100),
    fecha_registro DATETIME DEFAULT GETDATE(),
    estado BIT DEFAULT 1,

    FOREIGN KEY (id_rol) REFERENCES Roles(id_rol)
);


CREATE TABLE Mascotas (
    id_mascota INT IDENTITY PRIMARY KEY,
    id_usuario_publicador INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    edad INT,
    tipo VARCHAR(20), -- Perro / Gato
    raza VARCHAR(100),
    sexo VARCHAR(10),
    estado_salud VARCHAR(100),
    descripcion TEXT,
    ubicacion VARCHAR(150),
    estado_adopcion VARCHAR(30) DEFAULT 'Disponible',
    fecha_publicacion DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (id_usuario_publicador) REFERENCES Usuarios(id_usuario)
);


CREATE TABLE Mascota_Fotos (
    id_foto INT IDENTITY PRIMARY KEY,
    id_mascota INT NOT NULL,
    url_foto VARCHAR(255) NOT NULL,
    es_principal BIT DEFAULT 0,

    FOREIGN KEY (id_mascota) REFERENCES Mascotas(id_mascota)
);


CREATE TABLE Solicitudes_Adopcion (
    id_solicitud INT IDENTITY PRIMARY KEY,
    id_mascota INT NOT NULL,
    id_usuario_solicitante INT NOT NULL,
    estado VARCHAR(30) DEFAULT 'En revisión',
    mensaje TEXT,
    fecha_solicitud DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (id_mascota) REFERENCES Mascotas(id_mascota),
    FOREIGN KEY (id_usuario_solicitante) REFERENCES Usuarios(id_usuario)
);


CREATE TABLE Documentos_Usuario (
    id_documento INT IDENTITY PRIMARY KEY,
    id_usuario INT NOT NULL,
    tipo_documento VARCHAR(50) NOT NULL,
    url_archivo VARCHAR(255) NOT NULL,
    estado_verificacion VARCHAR(30) DEFAULT 'Pendiente',
    fecha_subida DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);


CREATE TABLE Adopciones (
    id_adopcion INT IDENTITY PRIMARY KEY,
    id_mascota INT NOT NULL,
    id_adoptante INT NOT NULL,
    fecha_adopcion DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (id_mascota) REFERENCES Mascotas(id_mascota),
    FOREIGN KEY (id_adoptante) REFERENCES Usuarios(id_usuario)
);





-- =========================
-- Insertar Usuarios
-- =========================
INSERT INTO Usuarios (id_rol, nombre, apellido, email, password_hash, telefono, direccion, ciudad)
VALUES 
(1, 'Carlos', 'Pérez', 'carlos.perez@example.com', 'hash123', '3001112233', 'Calle 10 #5-20', 'Bogotá'),
(2, 'María', 'Gómez', 'maria.gomez@example.com', 'hash456', '3012223344', 'Carrera 15 #45-60', 'Medellín');

-- =========================
-- Insertar Mascotas
-- =========================
INSERT INTO Mascotas (id_usuario_publicador, nombre, edad, tipo, raza, sexo, estado_salud, descripcion, ubicacion)
VALUES
(1, 'Firulais', 3, 'Perro', 'Labrador', 'Macho', 'Saludable', 'Perro juguetón y cariñoso', 'Bogotá'),
(2, 'Misu', 2, 'Gato', 'Siames', 'Hembra', 'Vacunada', 'Gata tranquila y casera', 'Medellín');

-- =========================
-- Insertar Mascota_Fotos
-- =========================
INSERT INTO Mascota_Fotos (id_mascota, url_foto, es_principal)
VALUES
(1, 'https://example.com/firulais1.jpg', 1),
(2, 'https://example.com/misu1.jpg', 1);

-- =========================
-- Insertar Solicitudes_Adopcion
-- =========================
INSERT INTO Solicitudes_Adopcion (id_mascota, id_usuario_solicitante, mensaje)
VALUES
(1, 2, 'Estoy interesado en adoptar a Firulais.'),
(2, 1, 'Quiero darle un hogar a Misu.');

-- =========================
-- Insertar Documentos_Usuario
-- =========================
INSERT INTO Documentos_Usuario (id_usuario, tipo_documento, url_archivo)
VALUES
(1, 'Cédula', 'https://example.com/carlos_cedula.pdf'),
(2, 'Cédula', 'https://example.com/maria_cedula.pdf');

-- =========================
-- Insertar Adopciones
-- =========================
INSERT INTO Adopciones (id_mascota, id_adoptante)
VALUES
(1, 2),
(2, 1);





select * from Roles;