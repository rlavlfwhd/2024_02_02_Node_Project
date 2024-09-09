let http = require("http");

http.createServer(function (request, response){

    response.writeHead(200, {'content-Type' : 'text/plain'});

    response.end('Hello world');
}).listen(8000);

console.log("Server running at http://127.0.0.1:8000");