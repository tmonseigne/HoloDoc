var express = require('express');
var router = express.Router();
const fs = require('fs');
const cv = require('opencv4nodejs');

const utils = require('./route-utils');
const improc = require('../improc/improc');

const dal = require('../database/dal.js');


// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

// Get all the linked documents for a given document
router.get('/connected', function (req, res) {
  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    if (params && params.firstId && params.secondId)
    {

      let first = params.firstId;
      let second = params.secondId;

      dal.areConnected(function (connected) {
        res.status(200).send({ connected: connected });
      });
    }
    else
    {
      res.status(500).send({ error: 'wrong parameters' });
    }
  });
});

function documentValidParameters(params) {
  if(!params)
  {
    return false;
  }

  return params.name && params.label && params.desc && params.author && params.date;
}

// Add a new document into the database
router.post('/matchorcreate', function (req, res) {
  console.log('document - post - /matchorcreate');
  utils.asyncGetDataStream(req, function(buffer) {
    if (buffer && buffer.length > 0 ) {

      let params = JSON.parse(buffer);

      dal.getDocumentCount().then(function (count) {
        let number = count;

        let buf = Buffer.from(params.image, 'hex');
        let image = improc.streamToMat(buf);

        let bidule = improc.detectDocuments(image, [25, 25, 25]);
        if (bidule.length > 0) {
          let center = improc.getCenter(image);
          let toExtract = improc.getNearestdocFrom(bidule, center);

          let croped = improc.undistordDoc(image, toExtract);
          let features = improc.extractFeatures(croped);

          // 1. Need to find a match.
          // 1.1 First we compute the features for the document.
          // 1.2 We compare these features with all of a part of the features in the database.
          // 1.3 If the feature distance is below a threshold we have a match, else not.
          dal.matchFeatures(features, function(matchedDocument) {
            console.log("bloup");
          	if (matchedDocument)
          	{
          	  // 2. If matched we return the matched document information.
          	  // 2.1 The match call a callback function by passing the finded document in paramaters, we just have to return this to the client.
              let result = {
                id: matchedDocument._id,
                name: matchedDocument.name,
                label: matchedDocument.label,
                desc: matchedDocument.desc,
                author: matchedDocument.author,
                path: matchedDocument.path,
                image: improc.matToBase64(cv.imread(matchedDocument.path))
              }

          	  res.status(200).json(result);
          	}
          	else
          	{
              console.log("la");
          	  // 3. Else we create then add the new document to the database and then return all the information.
          	  // 3.1 First creation of the document in the database.
          	  // 3.2 Then writing the image on disk.
          	  let path = './data/' + number + '.png';
          	  dal.createDocument(String(number), 'undefined', '',
      			     'undefined', Date.Now, path,
      			     features,
      			     function (doc) {
                   let result = {
                     id: doc._id,
                     name: doc.name,
                     label: doc.label,
                     desc: doc.desc,
                     author: doc.author,
                     path: doc.path,
                     image: improc.matToBase64(croped)
                   }
      			       res.status(200).json(result);
      			     },
      			     function (err) {
      			       res.status(500).send({ error: err });
      			     }
    			    );

          	  improc.write(croped, path);
          	}
          });
        }
        else
        {
          res.status(500).send({ error: 'No documents found' });
        }
      });
    }
    else
    {
      res.status(500).send({ error: 'wrong parameters' });
    }
  });
});

// Update a document of the database
router.post('/update', function (req, res) {
  console.log('document - post - /update');

  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);

    if (params && documentValidParameters(params)) {
      dal.updateDocument(params.id, params,  function (doc) {
        res.status(200).json(doc);
      });
    }
    else
    {
      res.status(500).send({ error: 'wrong parameters' });
    }
  });

});



module.exports = router;
