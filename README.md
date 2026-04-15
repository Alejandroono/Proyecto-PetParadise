# 🐾 PetParadise

**PetParadise** es una plataforma web desarrollada en ASP.NET Core que facilita la adopción responsable de mascotas. Permite gestionar información de animales, usuarios y procesos de adopción de manera eficiente, integrando servicios modernos y buenas prácticas de desarrollo.

---

## 🚀 Características

- 🐶 Registro y gestión de mascotas
- 👤 Gestión de usuarios
- 📋 Proceso de adopción
- ☁️ Integración con Azure Storage
- 🧪 Pruebas unitarias implementadas
- 🏗️ Arquitectura por capas (Controllers, Services, Models)

---

## 🛠️ Tecnologías

- ASP.NET Core
- Entity Framework Core
- SQL Server
- Azure Storage
- xUnit

---

## ⚙️ Configuración del proyecto

Antes de ejecutar el proyecto, es necesario configurar el archivo `appsettings.json`.

---

### 🔐 Cadena de conexión a la base de datos

Debes agregar tu cadena de conexión de SQL Server en:

```json
"ConnectionStrings": {
  "servidor": "",
  "AzureStorage": ""
}
