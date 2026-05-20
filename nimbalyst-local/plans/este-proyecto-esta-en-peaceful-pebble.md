# Plan: Exponer Calorix en la escuela (con código a la mano)

## Contexto

Calorix es una app ASP.NET Core 9 MVC con SQLite que actualmente corre en tu PC de casa (`C:\Users\ikere\Documents\Calorix\Calorix`). Necesitas (1) **demostrarla funcionando** en una PC de la escuela que no controlas y (2) **tener el código fuente disponible** por si te lo piden.

**Hallazgos clave que hacen esto fácil:**
- La conexión a la BD usa path relativo (`Data Source=calorix.db`) → 100% portable
- `DbInitializer` auto-crea y siembra la BD al primer arranque → no requiere setup manual
- No hay rutas absolutas, no requiere SQL Server, no requiere Docker
- Ya existe `origin` apuntando a `https://github.com/claudecalorix-coder/Calorix.git`

**Riesgos de la PC escolar:**
- Puede no tener .NET 9 SDK instalado
- Puede no haber permisos de admin para instalar nada
- Firewall escolar suele bloquear túneles (ngrok / cloudflared)
- SmartScreen puede advertir al correr un .exe sin firmar

## Estrategia: USB con build self-contained + GitHub como respaldo

**Por qué esta combinación:** El ejecutable self-contained funciona sin SDK, sin admin, sin internet. GitHub te da acceso al código fuente desde cualquier navegador si te lo piden. Túneles y cloud son frágiles para una presentación calificada.

## Plan A (PRIMARIO) — USB con `dotnet publish` self-contained

### 1. Publicar el build self-contained (en casa)

```powershell
cd C:\Users\ikere\Documents\Calorix\Calorix
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o C:\Users\ikere\Desktop\Calorix-Demo
```

Esto produce una carpeta con `Calorix.exe` (~80–150 MB) que **incluye el runtime de .NET 9 embebido**. No requiere instalar nada en la PC de destino.

### 2. Decidir el estado de la base de datos

Antes de copiar al USB, elige uno (o lleva ambos en carpetas separadas):

- **BD limpia (recomendado para demo):** elimina `calorix.db` de la carpeta publish. `DbInitializer` la recreará al arrancar con datos de seed predecibles.
- **BD con tus datos:** copia tu `calorix.db` actual a la carpeta publish. Útil si tienes registros propios que mostrar.

### 3. Probar el .exe en OTRA PC antes de ir a la escuela

Esto es crítico. Copia `Calorix-Demo` a otra computadora (o cuenta sin admin) y verifica:

1. Doble click en `Calorix.exe` (acepta SmartScreen si aparece: "Más información" → "Ejecutar de todas formas")
2. Se abre una consola de Kestrel — déjala abierta
3. Abre el navegador en `http://localhost:5080`
4. Verifica login, navegación, datos visibles

Si falla aquí, falla en la escuela.

### 4. Preparar el USB

```
USB:\Calorix\
  ├── Demo-LimpioDB\        (publish con BD vacía, se auto-siembra)
  ├── Demo-ConDatos\        (publish con tu calorix.db actual)
  ├── Source\               (código fuente, sin bin/ ni obj/)
  └── COMO-CORRER.txt       (instrucciones de 3 líneas)
```

Para `Source\`: clona limpio desde GitHub después del push, o copia tu carpeta excluyendo `bin/`, `obj/`, `.vs/`, `nimbalyst-local/`.

### 5. En la escuela

1. Conecta el USB
2. Entra a `USB:\Calorix\Demo-LimpioDB\` (o el que decidas)
3. Doble click en `Calorix.exe`
4. Abre el navegador en `http://localhost:5080`
5. Listo para demostrar

## Plan B (RESPALDO) — GitHub

### Antes de ir a la escuela: empujar los commits locales

Tu remote `origin` ya está configurado, pero los commits recientes (incluyendo `Add Calorix nutrition tracking app`) pueden no estar pusheados. Verifica y empuja:

```powershell
git status
git log origin/master..HEAD --oneline   # commits locales sin pushear
git push origin master
```

**Antes del push, revisa `.gitignore`** para que excluya `bin/`, `obj/`, y considera si quieres incluir `calorix.db` en el repo (si tiene datos personales, agrégalo al `.gitignore`).

### Si Plan A falla en la PC escolar

1. Verifica si la PC tiene .NET: abre PowerShell y ejecuta `dotnet --version`
2. **Si tiene .NET 9 SDK:** clona desde GitHub o copia `Source\` del USB, luego `dotnet run`
3. **Si no tiene .NET:** vuelve al `Demo-*` del USB (self-contained no necesita SDK)
4. **Si bloquean ejecutables completamente:** abre la URL del repo en el navegador para mostrar el código

## Lo que NO recomiendo (y por qué)

- **Ngrok / Cloudflare Tunnel:** firewalls escolares casi siempre bloquean `*.ngrok.io` y `*.trycloudflare.com`. Además dependes de que tu PC de casa esté encendida.
- **Cloud hosting (Azure/Render/Railway):** SQLite en cloud requiere configurar volumen persistente o pierdes la BD en cada redeploy. Demasiado setup para una demo de clase.

## Verificación end-to-end (la noche antes)

- [ ] `dotnet publish` ejecutado, carpeta `Calorix-Demo` generada y pesa ~100 MB
- [ ] `Calorix.exe` probado en otra PC (idealmente sin permisos de admin)
- [ ] `http://localhost:5080` carga correctamente y permite login
- [ ] `git push origin master` realizado, repo visible en github.com
- [ ] USB contiene `Demo-LimpioDB\`, `Demo-ConDatos\`, `Source\` y un README
- [ ] URL del repo de GitHub anotada en papel/teléfono (respaldo si pierdes el USB)
- [ ] Capturas de pantalla de la app funcionando guardadas en el USB (último recurso si todo falla)

## Archivos críticos referenciados

- `C:\Users\ikere\Documents\Calorix\Calorix\Calorix.csproj` — target framework net9.0
- `C:\Users\ikere\Documents\Calorix\Calorix\Properties\launchSettings.json` — puerto 5080
- `C:\Users\ikere\Documents\Calorix\Calorix\appsettings.json` — connection string a `calorix.db` (path relativo)
- `C:\Users\ikere\Documents\Calorix\Calorix\Program.cs` — donde `DbInitializer` siembra la BD
- `C:\Users\ikere\Documents\Calorix\Calorix\.gitignore` — revisar antes de push
