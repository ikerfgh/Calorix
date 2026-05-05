---
title: Conectar proyecto a GitHub (repo nuevo)
date: 2026-05-04T00:00:00.000Z
status: pending
---
## Context

El proyecto en `C:\Users\ikere\Documents\Calorix\Calorix` no tiene git inicializado ni repositorio en GitHub. El objetivo es:
1. Autenticar la máquina con GitHub
2. Inicializar git localmente
3. Crear un repositorio nuevo en GitHub
4. Subir el proyecto

---

## Plan

### Paso 1 — Autenticar con GitHub CLI

Instalar `gh` si no está disponible, luego autenticar:

```bash
gh auth login
```

El asistente interactivo preguntará:
- **GitHub.com** o GitHub Enterprise → elegir **GitHub.com**
- Protocolo: **HTTPS** (más simple) o SSH
- Autenticación: **Login with a web browser** (abre el navegador y autoriza)

Verificar autenticación:
```bash
gh auth status
```

---

### Paso 2 — Inicializar git en el proyecto

```bash
git init
git add .
git commit -m "Initial commit"
```

---

### Paso 3 — Crear el repositorio en GitHub y conectarlo

Con `gh` CLI (crea el repo Y configura el remote automáticamente):

```bash
gh repo create Calorix --public --source=. --remote=origin --push
```

Opciones clave:
- `--public` → repositorio público (cambiar a `--private` si quieres privado)
- `--source=.` → usa la carpeta actual como fuente
- `--push` → hace push del commit inicial automáticamente

---

### Paso 4 — Verificar

```bash
git remote -v          # debe mostrar origin apuntando a github.com
git log --oneline      # debe mostrar el commit inicial
gh repo view --web     # abre el repo en el navegador
```

---

## Archivos críticos

- `.git/config` — se crea en el Paso 2, contiene la URL del remote tras el Paso 3
- `C:\Users\ikere\Documents\Calorix\Calorix` — directorio raíz del proyecto

---

## Prerrequisitos

- `git` instalado (verificar con `git --version`)
- `gh` CLI instalado (descargar en https://cli.github.com si no está)
- Cuenta en GitHub con email `claudecalorix@gmail.com`

---

## Verificación final

- `gh repo view --web` abre el repositorio en GitHub con los archivos del proyecto
- El botón "Commit" en Nimbalyst funciona correctamente (requiere git + remote configurados)
