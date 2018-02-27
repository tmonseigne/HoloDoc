const cv = require('opencv4nodejs');
const improc = require ('../improc/improc');
const utils = require('../routes/route-utils');


var PORT = 44444;
var HOST = '127.0.0.1';

var net = require('net');

var server = net.createServer(function(socket) {
  console.log("connected !");

  socket.on('data', onSocketData.bind(socket));

  socket.dataSize = 0;
  socket.readBytes = 0;
  socket.myData = Buffer.alloc(0);
  
});

function onSocketData (data) {
  let buf = Buffer.from(data, 'base64');
  console.log(buf.length);
  let newData = buf.length == 4;
  
  if (newData) {
    this.dataSize = buf.readInt32LE();
    this.readBytes = 0;

    console.log("New Data Size: " + this.dataSize);
    
  } else if (this.dataSize > 0) {
    let toRead = Math.min(buf.length, this.dataSize - this.readBytes);
    
    this.readBytes += toRead;
    this.myData = Buffer.concat([this.myData, buf], this.readBytes);

    if (this.readBytes >= this.dataSize) {

      console.log("Data Read: " + this.readBytes);
      
      let image = cv.imdecode(this.myData);

      improc.write(image, 'bla' + '.jpg');
      
      let result = improc.detectDocuments(image, new cv.Vec(0,0,0), new cv.Vec(50,50,50));

      let buffer = pointArrayToBuffer(result);
      
      let resultBuf = Buffer.alloc(4);
      resultBuf.writeInt32LE(buffer.length, 0);

      console.log('Amout of sended data: ' + buffer.length);
      //server.send(resultBuf, 0, resultBuf.length, remote.port, remote.adress);
      this.write(resultBuf);
      
      console.log(result[0]);
      //server.send(buffer, 0, buffer.length, remote.port, remote.adress);
      this.write(buffer);

      this.myData = Buffer.alloc(0);
      this.dataSize = 0;
    }
  }
}

function pointArrayToBuffer(points) {
  let nbBytes = points.length * 2 * 4;

  let buf = Buffer.alloc(nbBytes);

  for (let i = 0; i < points.length; ++i) {
    let index = i * 8;
    buf.writeInt32LE(points[i].x, index);
    buf.writeInt32LE(points[i].y, index + 4);
  }

  return buf;
}


server.listen(PORT, HOST, function() {
  console.log('Server listening on', server.address());
});
			     
