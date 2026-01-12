# Vorluno · Formulario de Contacto

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-cyan.svg)](https://reactjs.org/)

Formulario de contacto moderno y elegante para Vorluno, construido con ASP.NET Core 9 y React 18 + Vite.

## 🎨 Características

- **Diseño Moderno**: Paleta Vorluno (#7C3AED violet, #06B6D4 cyan, #0F172A slate)
- **Form Validation**: React Hook Form + Zod
- **Email Notifications**: SMTP via Brevo
- **Offline Support**: Reintentos automáticos cuando vuelve la conexión
- **Responsive**: Mobile-first design con glassmorphism
- **Accesibilidad**: ARIA labels, keyboard navigation

## 📁 Estructura del Proyecto

```
.
├── src/
│   ├── api/                    # Backend ASP.NET Core
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   └── web/                    # Frontend React + Vite
│       ├── src/
│       │   ├── components/
│       │   └── lib/
│       └── package.json
├── docs/                       # Documentación
├── scripts/                    # Scripts de utilidad
└── README.md
```

## 🚀 Inicio Rápido

### Prerequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- Cuenta en [Brevo](https://www.brevo.com/) para SMTP

### 1. Configurar Variables de Entorno

Crear `src/api/appsettings.Development.json` (o usar User Secrets):

```json
{
  "Email": {
    "Smtp": {
      "User": "TU_USUARIO_BREVO",
      "Password": "TU_PASSWORD_BREVO"
    }
  }
}
```

### 2. Ejecutar Backend

```bash
cd src/api
dotnet restore
dotnet run
```

El backend estará disponible en `https://localhost:7150`

### 3. Ejecutar Frontend

```bash
cd src/web
npm install
npm run dev
```

El frontend estará disponible en `http://localhost:5173`

## 🎨 Branding

### Colores Vorluno

```css
--vorluno-violet: #7C3AED;  /* Creatividad, innovación */
--vorluno-cyan: #06B6D4;    /* Modernidad, tecnología */
--vorluno-slate: #0F172A;   /* Profesionalismo, confianza */
```

### Logo

El logo debe estar en:
- `src/api/wwwroot/email-assets/vorluno-logo.png`
- `src/web/public/email-assets/vorluno-logo.png`

Formato recomendado: PNG con transparencia, ~200x60px

## 📧 Configuración de Email

### From Address
- `contacto@vorluno.dev` (requiere verificación en Brevo)

### To Address
- `vorluno@gmail.com` (inbox de leads)

### SMTP Settings (Brevo)
- Host: `smtp-relay.brevo.com`
- Port: `2525`
- TLS: `true`

## 🔧 Scripts Útiles

### Verificar referencias a "CLAU"

```bash
cd scripts
./check-anti-clau.sh
```

### Build de Producción

```bash
# Backend
cd src/api
dotnet publish -c Release -o ./publish

# Frontend
cd src/web
npm run build
```

## 🧪 Testing

_No hay tests aún. TODO: Agregar tests unitarios y de integración._

## 📝 Convenciones Vorluno

- **Repositorio**: `vorluno/contacto`
- **Subdomain**: `contacto.vorluno.dev`
- **ID Interno**: `VOR-CONTACTO`
- **Namespaces**: `Vorluno.Contacto.Api`

---

**Made with ❤️ by Vorluno · Transformando ideas en realidad**