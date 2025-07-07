# MarketAsset.API

This is a .NET 8 Web API that integrates with the Fintacharts platform.  
It retrieves market assets and their historical prices, stores them in a local SQLite database, and updates real-time prices via WebSocket.

---

## How to Run the Application via Docker

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running
- (Optional) Visual Studio 2022+ with Docker support enabled

---

## Option 1: Run Using Visual Studio (Recommended)

1. Open the solution in **Visual Studio**
2. Make sure Docker support is enabled for the `MarketAsset.API` project (already done)
3. In the top dropdown near the ▶️ button, select:
   ```
   Docker (MarketAsset.API)
   ```
4. Press **F5** or click ▶️ to run

Visual Studio will:
- Build and run the Docker container
- Expose the API on a random local port
- Open Swagger automatically in your browser

📌 Example address:
```
http://localhost:32774/swagger
```

---

## Option 2: Run via Docker CLI (manual)

1. Open a terminal (PowerShell, bash, etc.)
2. Navigate to the folder where the `Dockerfile` is located

```bash
cd MarketAsset.API
```

3. Build the Docker image:

```bash
docker build -t market-asset-api .
```

4. Run the container with a fixed port:

```bash
docker run -d -p 8080:8080 --name market-api market-asset-api
```

5. Open Swagger in your browser:

```
http://localhost:8080/swagger
```

---

## SQLite Database

The database file is stored in the container at:

```
/app.db
```

## Configuration

Application settings (API keys, endpoints, connection string) are located in:

```
appsettings.json
```

To change configuration inside Docker, you can override with environment variables (optional).

---

## API Reference

- Swagger UI: `http://localhost:8080/swagger`