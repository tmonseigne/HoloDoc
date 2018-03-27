var express = require('express');
var router = express.Router();
const fs = require('fs');
const cv = require('/usr/lib/node_modules/opencv4nodejs');

const utils = require('./route-utils');
const improc = require('../improc/improc');

const dal = require('../database/dal.js');

var BackGroundColor = [25, 25, 25];
// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

router.post('/background', function (req, res) {
  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    if (params && params.R && params.G && params.B)
    {
      BackGroundColor = [params.B, params.G, params.R];
      res.status(200).send(params);
    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });
});

// Get all the linked documents for a given document
router.post('/connected', function (req, res) {
  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    if (params && params.firstId && params.secondId)
    {

      let first = params.firstId;
      let second = params.secondId;

      dal.areConnected(function (connected) {
        res.status(200).send({ Connected: connected });
      });
    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });
});

function documentValidParameters(params) {
  if(!params)
  {
    return false;
  }

  return params.label && params.author && params.date && params.id;
}

function saveDocument (image, features, res, number) {
  let path = './data/' + number + '.png';
  dal.createDocument(String(number), 'undefined', '',
     'undefined', Date.Now, path,
     features,
     function (doc) {
       let result = {
         Id: doc._id,
         Name: doc.name,
         Label: doc.label,
         Desc: doc.desc,
         Author: doc.author,
         Path: doc.path,
         Image: improc.matToBase64(image)
       }
       res.status(200).json(result);
     },
     function (err) {
       res.status(500).send({ "Error": err });
     }
  );

  improc.write(image, path);
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
        cv.imwrite('test.jpg', image);

        let toSave = image;
        let bidule = improc.detectDocuments(image, BackGroundColor);

        let detected = false;
        if (bidule.length > 0) {
          detected = true;
          let center = improc.getCenter(image);
          let toExtract = improc.getNearestdocFrom(bidule, center);

          let croped = improc.undistordDoc(image, toExtract);
          toSave = croped;
        }

        let features = improc.extractFeatures(toSave);

        // 1. Need to find a match.
        // 1.1 First we compute the features for the document.
        // 1.2 We compare these features with all of a part of the features in the database.
        // 1.3 If the feature distance is below a threshold we have a match, else not.
        if (detected) {
          dal.matchFeatures(features, function(matchedDocument) {
          	if (matchedDocument)
          	{
          	  // 2. If matched we return the matched document information.
          	  // 2.1 The match call a callback function by passing the finded document in paramaters, we just have to return this to the client.
              dal.getLink(matchedDocument._id, function (link) {
                let result = {
                  Id: matchedDocument._id,
                  Name: matchedDocument.name,
                  Label: matchedDocument.label,
                  Desc: matchedDocument.desc,
                  Author: matchedDocument.author,
                  Path: matchedDocument.path,
                  Link: link ? link.objects : undefined,
                  Image: improc.matToBase64(cv.imread(matchedDocument.path))
                }

            	  res.status(200).json(result);
              });
          	}
          	else
          	{
          	  // 3. Else we create then add the new document to the database and then return all the information.
          	  // 3.1 First creation of the document in the database.
          	  // 3.2 Then writing the image on disk.
              saveDocument(toSave, features, res, number);
          	}
          });
        }
        else
        {
          saveDocument(toSave, features, res, number);
        }


      });
    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });
});

// Update a document of the database
router.post('/update', function (req, res) {
  console.log('document - post - /update');

  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    console.log(params);
    if (params && documentValidParameters(params)) {
      dal.updateDocument(params.id, params,  function (doc) {
        res.status(200).json(doc);
      });
    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });

});

// Update a document of the database
router.post('/updatephoto', function (req, res) {
  console.log('document - post - /updatephoto');

  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);

    if (params && params.id && params.image) {
      dal.getDocuments({_id: params.id}, function (err, docs) {
        if (docs.length > 0) {
          let path = docs[0].path;

          let buf = Buffer.from(params.image, 'hex');
          let image = improc.streamToMat(buf);
          cv.imwrite('test.jpg', image);

          let bidule = improc.detectDocuments(image, BackGroundColor);

          let toSave = image;
          if (bidule.length > 0)
          {
            let center = improc.getCenter(image);
            let toExtract = improc.getNearestdocFrom(bidule, center);

            let croped = improc.undistordDoc(image, toExtract);
            toSave = croped;
          }


          let features = improc.extractFeatures(toSave);

          dal.updateDocument(params.id, {features: features}, function (doc) {
            let result = {
              Id: doc._id,
              Name: doc.name,
              Label: doc.label,
              Desc: doc.desc,
              Author: doc.author,
              Path: doc.path,
              Image: improc.matToBase64(toSave)
            };
            res.status(200).json(result);
            cv.imwrite(path, toSave);
          });
        } else {
          res.status(500).send({ "Error": 'The document does not exist' });
        }
      });
    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });

});



module.exports = router;
