# Deployment Guide - Vorluno Contact Form

## CapRover Deployment

Este proyecto está configurado para desplegarse en **CapRover** usando Docker.

### Dominio
- **Producción**: `contacto.vorluno.dev`
- **Base CapRover**: `*.apps.vorluno.dev`

---

## 1. Requisitos Previos

- CapRover instalado y configurado
- Dominio `contacto.vorluno.dev` apuntando a tu servidor CapRover
- Cuenta de Brevo con API key válida
- Logo de Vorluno accesible públicamente

---

## 2. Variables de Entorno Requeridas

Configura estas variables en la sección "App Configs" de CapRover:

### Email Configuration (Brevo)

```bash
# Brevo API Key (OBLIGATORIO)
Email__Brevo__ApiKey=xkeysib-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

# Email remitente (OBLIGATORIO)
Email__From=contacto@vorluno.dev

# Email destinatario para notificaciones internas (OBLIGATORIO)
Email__To=vorluno@gmail.com

# URL del logo de Vorluno (OBLIGATORIO)
Email__Logo__Url=https://vorluno.dev/assets/vorluno-logo.png

# Email remitente para acuses de recibo (OPCIONAL, usa Email__From si no se especifica)
Email__Ack__From=contacto@vorluno.dev

# URL de la imagen hero para acuses (OPCIONAL)
Email__Ack__Hero__Url=https://vorluno.dev/assets/vorluno-hero.png
```

### Google Sheets Integration (Opcional)

```bash
# URL del webhook de Google Sheets (OPCIONAL)
GoogleSheets__WebhookUrl=https://script.google.com/macros/s/XXXXX/exec

# Token de autenticación para el webhook (OPCIONAL)
GoogleSheets__Token=tu-token-secreto
```

### Otras Variables

```bash
# Ambiente (automático en CapRover)
ASPNETCORE_ENVIRONMENT=Production

# Puerto (CapRover lo inyecta automáticamente)
PORT=8080
```

---

## 3. Configuración en CapRover

### 3.1 Crear la App

1. Ve a **Apps** en tu panel de CapRover
2. Clic en **Create New App**
3. Nombre: `contacto`
4. Clic en **Create New App**

### 3.2 Habilitar HTTPS

1. Ve a la app `contacto`
2. En la sección **HTTP Settings**:
   - Habilita **HTTPS**
   - Habilita **Force HTTPS**
   - Habilita **Websocket Support** (opcional)
3. Clic en **Save & Update**

### 3.3 Configurar Dominio

1. En la sección **HTTP Settings**:
   - Agrega dominio personalizado: `contacto.vorluno.dev`
   - Clic en **Connect New Domain**
   - Habilita **Enable HTTPS** para el dominio
2. CapRover automáticamente generará certificado SSL con Let's Encrypt

### 3.4 Configurar Variables de Entorno

1. Ve a la sección **App Configs**
2. Agrega cada variable de entorno del paso 2
3. Formato para ASP.NET Core: usa `__` (doble underscore) para separar niveles
   - Ejemplo: `Email__Brevo__ApiKey` se traduce a `Email:Brevo:ApiKey` en JSON
4. Clic en **Add** para cada variable
5. Clic en **Save & Update**

### 3.5 Deploy

**Opción A: Deploy desde Git (Recomendado)**

1. Ve a **Deployment** tab
2. Selecciona **Method 3: Deploy from Github/Bitbucket/Gitlab**
3. Configura:
   - Repository: tu repositorio Git
   - Branch: `main` o `master`
   - Username/Password o Deploy Token
4. Clic en **Save & Update**

**Opción B: Deploy Manual**

```bash
# Desde la raíz del proyecto
caprover deploy

# O específicamente
caprover deploy -a contacto
```

---

## 4. Verificación del Deployment

### 4.1 Health Check

CapRover automáticamente verificará el endpoint `/healthz` cada 30 segundos.

Verifica manualmente:

```bash
curl https://contacto.vorluno.dev/healthz
# Respuesta esperada: Healthy
```

### 4.2 Prueba del Formulario

1. Abre `https://contacto.vorluno.dev` en tu navegador
2. Llena el formulario de contacto
3. Envía el formulario
4. Verifica:
   - Email interno recibido en `Email__To`
   - Email de acuse recibido en el email del formulario
   - Logs en CapRover sin errores

### 4.3 Ver Logs

En CapRover:
1. Ve a la app `contacto`
2. Clic en **View Logs**
3. Verifica que no haya errores

```bash
# O desde CLI
caprover logs -a contacto --follow
```

---

## 5. Testing Local con Docker

### 5.1 Build de la Imagen

```bash
# Desde la raíz del proyecto
docker build -t vorluno-contacto:test .
```

### 5.2 Run Local

```bash
docker run -d \
  -p 8080:8080 \
  -e Email__Brevo__ApiKey="tu-api-key" \
  -e Email__From="contacto@vorluno.dev" \
  -e Email__To="vorluno@gmail.com" \
  -e Email__Logo__Url="https://vorluno.dev/assets/vorluno-logo.png" \
  -e Email__Ack__From="contacto@vorluno.dev" \
  --name contacto-test \
  vorluno-contacto:test
```

### 5.3 Verificar

```bash
# Health check
curl http://localhost:8080/healthz

# Ver logs
docker logs -f contacto-test

# Prueba del formulario
# Abre http://localhost:8080 en tu navegador
```

### 5.4 Cleanup

```bash
docker stop contacto-test
docker rm contacto-test
docker rmi vorluno-contacto:test
```

---

## 6. Configuración de Brevo

### 6.1 Verificar Dominio

Para que los emails no caigan en spam:

1. Ve a **Settings > Senders & IP** en Brevo
2. Agrega el dominio `vorluno.dev`
3. Configura los registros DNS:
   - **SPF**: `v=spf1 include:spf.sendinblue.com ~all`
   - **DKIM**: Copia el registro TXT que Brevo te proporciona
   - **DMARC**: `v=DMARC1; p=none; rua=mailto:postmaster@vorluno.dev`

### 6.2 Verificar Remitentes

1. Ve a **Settings > Senders & IP**
2. Agrega y verifica:
   - `contacto@vorluno.dev`
3. Brevo enviará un email de confirmación

---

## 7. Troubleshooting

### Error: "Unable to send email. Your SMTP account is not yet activated"

**Solución**: Verifica tu cuenta de Brevo y activa el servicio SMTP.

### Error: "Port already in use"

**Solución**: CapRover inyecta automáticamente la variable `PORT`. No necesitas configurarla manualmente.

### Imágenes no cargan en emails

**Solución**:
- Verifica que `Email__Logo__Url` sea una URL pública accesible
- Las URLs deben usar `https://` no rutas relativas
- Prueba la URL en tu navegador

### CORS errors

**Solución**: El CORS ya está configurado para `https://contacto.vorluno.dev` en `appsettings.Production.json`

### SSL Certificate issues

**Solución**:
- Asegúrate de que el dominio apunte correctamente a CapRover
- Espera 2-3 minutos después de habilitar HTTPS
- CapRover renovará automáticamente el certificado

---

## 8. Endpoints Importantes

| Endpoint | Descripción | Método |
|----------|-------------|--------|
| `/` | Formulario web (SPA) | GET |
| `/api/contacto` | Envío de formulario | POST |
| `/healthz` | Health check | GET |
| `/swagger` | API docs (solo desarrollo) | GET |

---

## 9. Monitoreo

### Logs en Producción

Los logs están disponibles en:
1. CapRover dashboard > App > View Logs
2. CLI: `caprover logs -a contacto --follow`

### Métricas

CapRover proporciona:
- CPU usage
- Memory usage
- Request count
- Response times

Accede desde: Dashboard > Apps > contacto > Monitoring

---

## 10. Actualizaciones

### Actualización Automática (CI/CD)

Si configuraste deploy desde Git:
1. Haz commit de tus cambios
2. Push a la rama configurada
3. CapRover automáticamente rebuildeará y desplegará

### Actualización Manual

```bash
# Desde la raíz del proyecto
caprover deploy -a contacto
```

---

## 11. Rollback

Si algo sale mal:

1. Ve a CapRover dashboard
2. Apps > contacto > Deployment
3. En "Previous Deployments", clic en el deployment anterior
4. Clic en "Rollback to this version"

---

## 12. Backup

### Variables de Entorno

Exporta las variables de entorno desde CapRover regularmente:
1. Apps > contacto > App Configs
2. Copia todas las variables
3. Guárdalas en un lugar seguro (1Password, etc.)

### Código

Usa Git para mantener backups del código:
```bash
git push origin main
```

---

## 13. Seguridad

### Secrets

- ✅ API Keys almacenadas en variables de entorno de CapRover
- ✅ No commitear credenciales en Git
- ✅ Usar HTTPS para todas las comunicaciones
- ✅ Aplicación corre como usuario no-root

### Headers de Seguridad

Ya configurados en `Program.cs`:
- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `X-Frame-Options: DENY`

---

## 14. Performance

### Optimizaciones ya configuradas:

- ✅ Multi-stage Docker build (imagen final ~90MB)
- ✅ Response compression (Brotli + Gzip)
- ✅ Static file caching (1 año para assets, 10 min para HTML)
- ✅ Alpine Linux base images
- ✅ Health checks automáticos

---

## 15. Soporte

Para problemas o preguntas:
1. Revisa los logs en CapRover
2. Verifica las variables de entorno
3. Consulta este documento
4. Contacta al equipo de desarrollo

---

**Última actualización**: 2026-01-14
