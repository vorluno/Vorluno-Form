# Guía de Deploy en CapRover

## Configuración de Variables de Entorno en CapRover

Después de crear la app en CapRover, configura las siguientes variables de entorno:

### Variables Requeridas

```bash
# Email SMTP (Brevo)
Email__Smtp__User=TU_USUARIO_BREVO
Email__Smtp__Password=TU_PASSWORD_BREVO

# CORS (opcional, ajustar según dominio)
Cors__ProdOrigin=https://contacto.vorluno.dev

# Google Sheets Webhook (opcional)
GoogleSheets__WebhookUrl=TU_WEBHOOK_URL
GoogleSheets__Token=TU_TOKEN
```

## Pasos para Deploy

### 1. Crear App en CapRover

```bash
# Conectar a tu CapRover
caprover serversetup

# Crear nueva app
caprover create contacto-vorluno
```

### 2. Configurar Variables de Entorno

En el panel de CapRover:
1. Ve a tu app `contacto-vorluno`
2. Click en "App Configs"
3. Añade las variables de entorno listadas arriba
4. Click en "Save & Restart"

### 3. Deploy desde Git

```bash
# Inicializar repo si no está inicializado
git init
git add .
git commit -m "Initial commit"

# Añadir remote de CapRover
git remote add caprover https://captain.your-domain.com

# Push para deploy
git push caprover master
```

O usa el CLI:

```bash
caprover deploy
```

### 4. Configurar HTTPS

En CapRover:
1. Ve a "HTTP Settings"
2. Activa "HTTPS"
3. Activa "Force HTTPS"
4. Configura tu dominio personalizado (ej: `contacto.vorluno.dev`)

### 5. Verificar Deployment

Visita tu app en:
- `https://contacto-vorluno.captain.your-domain.com`
- O tu dominio personalizado: `https://contacto.vorluno.dev`

## Logs y Troubleshooting

Ver logs en tiempo real:
```bash
caprover logs contacto-vorluno
```

O en el panel de CapRover:
1. Ve a tu app
2. Click en "Deployment" tab
3. Revisa los logs de build y runtime

## Actualizar la App

```bash
git add .
git commit -m "Update: descripción del cambio"
git push caprover master
```

## Notas Importantes

- El puerto interno de la app es 80 (configurado en Dockerfile)
- CapRover automáticamente mapea esto al puerto público
- Las credenciales SMTP deben estar verificadas en Brevo
- El email "From" debe estar verificado en tu cuenta de Brevo
- El dominio debe apuntar a tu servidor CapRover vía DNS

## Configuración DNS

Para usar dominio personalizado:
```
A Record: contacto.vorluno.dev -> IP_DE_TU_SERVIDOR_CAPROVER
```

O si usas CNAME:
```
CNAME: contacto.vorluno.dev -> captain.your-domain.com
```
