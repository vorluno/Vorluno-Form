# Generación de Favicons para Vorluno

## Problema Detectado
Los favicons actuales tienen el logo de "CLAU" (azul oscuro y naranja), no de Vorluno.

## Solución

Necesitas generar nuevos favicons con el logo de Vorluno usando los colores oficiales:
- **Violet**: #7C3AED
- **Cyan**: #06B6D4
- **Slate**: #0F172A (fondo)

## Opción 1: Usar favicon.io (Recomendado)

1. Ve a https://favicon.io/favicon-converter/
2. Usa el logo existente `src/api/wwwroot/email-assets/vorluno-logo.png`
3. Descarga el paquete generado
4. Reemplaza los archivos en:
   - `src/web/public/`
   - `src/api/wwwroot/`

## Opción 2: Usar RealFaviconGenerator

1. Ve a https://realfavicongenerator.net/
2. Sube `src/api/wwwroot/email-assets/vorluno-logo.png`
3. Configura:
   - iOS App Icon: Fondo #0F172A
   - Android Chrome: Fondo #0F172A, Theme color #7C3AED
   - Windows Metro: Tile color #7C3AED
4. Descarga y reemplaza los archivos

## Opción 3: Usar SVG como Favicon (Moderno)

Ya creé `src/web/public/favicon.svg` con el logo simplificado de Vorluno.

Para navegadores modernos, agrega en `index.html`:
```html
<link rel="icon" type="image/svg+xml" href="/favicon.svg">
<link rel="alternate icon" href="/favicon.ico">
```

## Archivos a Reemplazar

En ambas carpetas (`src/web/public/` y `src/api/wwwroot/`):
- `favicon.ico` (16x16, 32x32 multi-resolución)
- `favicon-16x16.png`
- `favicon-32x32.png`
- `apple-touch-icon.png` (180x180)
- `android-chrome-192x192.png`
- `android-chrome-512x512.png`

## Colores Exactos para Generadores

- **Background**: #0F172A (slate oscuro)
- **Primary**: #7C3AED (violet)
- **Secondary**: #06B6D4 (cyan)
- **Text**: #FFFFFF (blanco)

## Verificación

Después de reemplazar, verifica:
1. Los favicons se ven en el navegador
2. El logo tiene los colores de Vorluno (violet-cyan)
3. No quedan referencias a "CLAU"
4. Los favicons funcionan en modo claro y oscuro

## Comando para Limpiar Caché del Navegador

```bash
# Limpia caché y fuerza recarga
Ctrl+Shift+R (Chrome/Firefox)
Cmd+Shift+R (Mac)
```
