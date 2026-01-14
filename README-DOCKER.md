# Docker Quick Start

## Quick Test (Automated)

```bash
# Make script executable
chmod +x docker-test.sh

# Run automated test
./docker-test.sh
```

## Manual Testing

### 1. Build

```bash
docker build -t vorluno-contacto:test .
```

### 2. Run

```bash
docker run -d \
  -p 8080:8080 \
  -e Email__Brevo__ApiKey="your-brevo-api-key" \
  -e Email__From="contacto@vorluno.dev" \
  -e Email__To="vorluno@gmail.com" \
  -e Email__Logo__Url="https://vorluno.dev/assets/vorluno-logo.png" \
  -e Email__Ack__From="contacto@vorluno.dev" \
  --name contacto-test \
  vorluno-contacto:test
```

### 3. Test

```bash
# Health check
curl http://localhost:8080/healthz

# Open browser
open http://localhost:8080
```

### 4. View Logs

```bash
docker logs -f contacto-test
```

### 5. Cleanup

```bash
docker stop contacto-test
docker rm contacto-test
docker rmi vorluno-contacto:test
```

## Environment Variables

See `.env.example` for all available variables.

**Required:**
- `Email__Brevo__ApiKey` - Your Brevo API key
- `Email__From` - Sender email address
- `Email__To` - Recipient for notifications
- `Email__Logo__Url` - Public URL of logo

**Optional:**
- `Email__Ack__From` - Sender for acknowledgment emails
- `Email__Ack__Hero__Url` - Hero image for acknowledgments
- `GoogleSheets__WebhookUrl` - Google Sheets webhook
- `GoogleSheets__Token` - Webhook auth token

## Ports

- **8080**: Application HTTP port (default)
- Can be changed via `PORT` environment variable

## Image Details

- **Base**: .NET 9 Alpine
- **Size**: ~90MB
- **User**: Non-root (uid 1000)
- **Health**: `/healthz` endpoint

## Troubleshooting

### Container won't start

```bash
# Check logs
docker logs contacto-test

# Check if port is already in use
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows
```

### Health check fails

```bash
# Check if app is listening
docker exec contacto-test netstat -tlnp

# Test health endpoint
curl -v http://localhost:8080/healthz
```

### Email errors

- Verify Brevo API key is correct
- Check sender email is verified in Brevo
- Review logs for error messages

## Production Deployment

For CapRover deployment, see [DEPLOYMENT.md](./DEPLOYMENT.md)
