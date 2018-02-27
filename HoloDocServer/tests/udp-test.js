const cv = require('opencv4nodejs');
const improc = require ('../improc/improc');
const utils = require('../routes/route-utils');


var PORT = 33333;
var HOST = '127.0.0.1';

var dgram = require('dgram');
var server = dgram.createSocket('udp4');

server.on('listening', function () {
  var address = server.address();
  console.log('UDP Server listening on ' + address.address + ":" + address.port);
});

var dataSize = 0;
var read = 0;

var data = Buffer.alloc(0);
var i = 0;


server.on('message', function (message, remote) {
  //console.log(remote.address + ':' + remote.port +' - ' + message.length);

  let buf = Buffer.from(message, 'base64');
  let newData = buf.length == 4;
  
  if (newData) {
    dataSize = buf.readInt32LE();
    read = 0;

    console.log("New Data Size: " + dataSize);
    
  } else if (dataSize > 0) {
    let toRead = Math.min(buf.length, dataSize - read);
    
    read += toRead;
    data = Buffer.concat([data, buf], read);

    if (read >= dataSize) {

      console.log("Data Read: " + read);
      
      let image = cv.imdecode(data);


      //improc.write(image, 'bla' + (i++) + '.jpg');
      
      let result = improc.detectDocuments(image, new cv.Vec(0,0,0), new cv.Vec(50,50,50));

      let buffer = pointArrayToBuffer(result);
      
      let resultBuf = Buffer.alloc(4);
      resultBuf.writeInt32LE(buffer.length, 0);

      console.log('Amout of sended data: ' + buffer.length);
      server.send(resultBuf, 0, resultBuf.length, remote.port, remote.adress);

      console.log(result[0]);
      server.send(buffer, 0, buffer.length, remote.port, remote.adress);
      

      data = Buffer.alloc(0);
    }
  }
});

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

server.on('error', (err) => {
  console.log(`server error:\n${err.stack}`);
  server.close();
});

server.bind(PORT, HOST);
