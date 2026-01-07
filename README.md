Task 1: Analysis & Project Setup

.NET 9 was downloaded via the internet and executed via CMD.

Navigated to /Desktop/WorkshopToDoManager/WorkshopToDoManager/ToDoManager and ran dotnet run.

The application started successfully.

Accessible and fully functional via the web UI at http://localhost:5172.

Task 2: Version Control (Git)

A public GitHub repository was created.

Commands used:
git init
git add .
git commit -m "Initialize repository and add .gitignore"

Repository was verified on GitHub and the branch set to main:
git branch -M main
git remote add origin https://github.com/ZeitschMarcus92/WorkshopToDoManager.git
git push -u origin main

Optionally, the branch can be renamed to master.

.gitignore was added and customized.

Task 3: Dockerization

The application was containerized using a multi-stage Dockerfile.

Build runs in a .NET SDK image, execution in a lightweight runtime image.

A .dockerignore file was created to exclude unnecessary files:
bin/, obj/, .git/.

Tested in Visual Studio using:
docker build -t todomanager:test .
docker run -p 8080:8080 todomanager:test

Since port 8080 was already in use, port 8081 was used successfully.

Task 4: Orchestration with Docker Compose

A docker-compose.yml was created with two services:

.NET web application

PostgreSQL database

Database data is persisted via a Docker volume.

Connection string is provided via the environment variable ConnectionStrings__ToDoDatabase.

A PostgreSQL health check ensures the app starts only after the DB is ready.

Tested successfully with:
docker compose up --build.

Task 5: GitHub Actions / CI (Continuous Integration)

A GitHub workflow was created at .github/workflows/ci.yml.

Pipeline triggers on:

Push to main

Pull requests

Manual trigger (workflow_dispatch)

Steps:

Checkout source code

dotnet build

dotnet test

Build Docker image

Upload Docker image as workflow artifact

Result: every code change is automatically validated with reproducible builds and tests.

Task 6: Bash Scripting – Emergency Backup Plan

A Bash script was created for manual execution.

Functionality:

Checks if the PostgreSQL container is running

Creates a database dump using docker exec and pg_dump

Stores backups in ./backups on the host

Filename includes a timestamp (backup_YYYY-MM-DD_HHMM.sql)

Bonus: keeps only the 5 most recent backups (log rotation)

Result: fast and secure manual DB backups without local PostgreSQL installation.

Task 7: Infrastructure as Code (Terraform)

Azure infrastructure was defined using Terraform.

Resources created:

Resource Group

App Service Plan

Linux Web App running a Docker container

nginx:latest is used as a placeholder image.

Internal container port 5172 is configured via WEBSITES_PORT.

Configuration was successfully validated using terraform init and terraform validate.