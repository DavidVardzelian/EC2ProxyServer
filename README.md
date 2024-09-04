
# Proxy Server in C#

## Description

This is a simple HTTP/HTTPS proxy server written in C#. It handles both HTTP and HTTPS requests, including the `CONNECT` method for tunneling. The server listens on port 6666 and can be used for various proxying purposes.

## Features

-   Handles HTTP and HTTPS requests
-   Supports the `CONNECT` method for HTTPS tunneling
-   Simple setup and usage

## Installation

### Prerequisites

-   [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine
-   Visual Studio or any C# IDE

### Clone the repository
```bash
 git clone https://github.com/your-username/your-repository.git
 cd your-repository
```
### Build and Run

```bash
 dotnet build
 dotnet run
```

## Configuration

The proxy server listens on port 6666 by default. If you want to change the port, modify the `Port` constant in the `ProxyServer` class.

## Usage

To use the proxy server, you can configure your HTTP/HTTPS client or browser to use `localhost:6666` as the proxy.

### Example HTTP Request

Using `curl`:

```bash
 curl -x http://localhost:6666 http://example.com
```
### Example HTTPS Request

Using `curl`:

```bash
 curl -x http://localhost:6666 https://example.com
```
## How It Works

1.  The server listens for incoming TCP connections on port 6666.
2.  When a client connects, it reads the request and determines if it is an HTTP request or an HTTPS `CONNECT` request.
3.  For HTTP requests, it forwards the request to the target server, receives the response, and sends it back to the client.
4.  For HTTPS `CONNECT` requests, it establishes a tunnel to the target server and facilitates the connection.

## Contributing

Feel free to submit pull requests or open issues for any bugs or enhancements.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions or support, please contact davidvardzelian@gmail.com