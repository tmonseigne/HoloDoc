

exports.asyncGetDataStream = function(req, onStreamEnded) {
  let body = [];

  req.on('data', function (chunk) {
      body.push(chunk);
  });

  req.on('end', function () {
      body = Buffer.concat(body);
      const buffer = Buffer.from(body,'base64');

      if (onStreamEnded) {
        onStreamEnded(buffer);
      }

  });
}
