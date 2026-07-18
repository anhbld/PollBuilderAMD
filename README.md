Prerequisites

Before running the project, install the following software:

.NET 8 SDK
Node.js (v20 or later)
Docker Desktop
Git
Getting Started
1. Clone the Repository
git clone https://github.com/anhbld/PollBuilderAMD.git

cd PollBuilderAMD
2. Running with Docker (Recommended)

Start all required services:

docker compose up --build

The containers will automatically start the frontend, backend, and database.

To stop the application:

docker compose down

3. Running Without Docker
Backend

Navigate to the backend folder.

cd backend

Restore dependencies.

dotnet restore

Apply database migrations.

dotnet ef database update

Run the API.

dotnet run

The backend will start on the configured localhost port.

Frontend

Open a second terminal.

cd frontend

Install dependencies.

npm install

Run the development server.

npm run dev

The frontend can then be accessed through the local Vite development server.
