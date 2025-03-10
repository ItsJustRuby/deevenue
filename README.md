# deevenue

Deevenue is a small imagebooru with an emphasis on speed and customization.

## Status

[![Lint](https://github.com/ItsJustRuby/deevenue/actions/workflows/lint.yml/badge.svg?branch=main)](https://github.com/ItsJustRuby/deevenue/actions/workflows/lint.yml)
[![Tests](https://github.com/ItsJustRuby/deevenue/actions/workflows/tests.yml/badge.svg?branch=main)](https://github.com/ItsJustRuby/deevenue/actions/workflows/tests.yml)
[![Deployment](https://github.com/ItsJustRuby/deevenue/actions/workflows/cd.yml/badge.svg?branch=main)](https://github.com/ItsJustRuby/deevenue/actions/workflows/cd.yml)
[![codecov](https://codecov.io/github/ItsJustRuby/deevenue/graph/badge.svg?token=EMC5OGYMAC)](https://codecov.io/github/ItsJustRuby/deevenue)

[![Bun](https://img.shields.io/badge/Bun-%23000000.svg?style=for-the-badge&logo=bun&logoColor=white)](https://bun.sh/)
[![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![ESLint](https://img.shields.io/badge/ESLint-4B3263?style=for-the-badge&logo=eslint&logoColor=white)](https://eslint.org/)
[![Prettier](https://img.shields.io/badge/prettier-%23F7B93E.svg?style=for-the-badge&logo=prettier&logoColor=black)](https://prettier.io/)
[![Svelte](https://img.shields.io/badge/svelte-%23f1413d.svg?style=for-the-badge&logo=svelte&logoColor=white)](https://svelte.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)](https://tailwindcss.com/)
[![Vite](https://img.shields.io/badge/vite-%23646CFF.svg?style=for-the-badge&logo=vite&logoColor=white)](https://vite.dev/)

## Development

### Setup

Local development uses docker compose. See [docker-compose.dev.yml](./docker-compose.dev.yml).

Customize your environment variables by copying [example.env](./example.env).

Common tasks are configured using [Task](https://github.com/go-task/task) in [Taskfile.yml](./Taskfile.yml). You probably want to run e.g. `task dev:run`.

### Migrations

If you make changes to the database models, the build will automatically fail until you have generated a matching migration file. Run `task dev:migrate` to generate the file, inspect it and commit it.
