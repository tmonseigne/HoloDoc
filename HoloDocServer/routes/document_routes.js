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


// define the home page route
// Will return all the documents
router.get('/',function (req, res) {
  console.log('document - get - /');
  res.status(500).send({ error: 'something blew up' });
});

router.post('/detect', function(req, res){
  console.log('document - get - /detect');

  utils.asyncGetDataStream(req, function(buffer) {
    const image = improc.streamToMat(buffer);
    let contours = improc.detectDocuments(image, new cv.Vec(0,0,0), new cv.Vec(50,50,50));

    res.status(200).json(contours);
  });
});


// Get all the linked documents for a given document
router.get('/connected', function (req, res) {
  if (req.params && req.params.firstId && req.params.secondId)
  {

    let first = req.params.firstId;
    let second = req.params.secondId;

    dal.areConnected(function (connected) {
      res.status(200).send({ connected: connected });
    });
  }
  else
  {
    res.status(500).send({ error: 'wrong parameters' });
  }
});

function newDocumentValidParameters(params) {
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
    if (buffer && buffer.length > 0 &&
	req.params && newDocumentValidParameters(req.params)) {

      let doc = req.params;
      let number = dal.documentCount();
      let features = [];
      const image = improc.streamToMat(buffer);
      
      // 1. Need to find a match.
      // 1.1 First we compute the features for the document.
      // 1.2 We compare these features with all of a part of the features in the database.
      // 1.3 If the feature distance is below a threshold we have a match, else not.
      dal.matchFeatures(features, function(matchedDocument) {
	
	if (matchedDocument)
	{
	  // 2. If matched we return the matched document information.
	  // 2.1 The match call a callback function by passing the finded document in paramaters, we just have to return this to the client.
	  res.status(200).json(document);
	}
	else
	{
	  // 3. Else we create then add the new document to the database and then return all the information.
	  // 3.1 First creation of the document in the database.
	  // 3.2 Then writing the image on disk.
	  let path = './data/' + number + '.png';
	  
	  dal.createDocument(doc.name, doc.label, doc.desc,
			     doc.author, doc.date, path,
			     features,
			     function (document) {
			       res.status(200).json(document);
			     },
			     function (err) {
			       res.status(500).send({ error: err });
			     }
			    );
	  
	  improc.write(image, path);
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
  res.status(500).send({ error: 'something blew up' });
});

// Add a link between two documents
router.post('/link', function (req, res) {
  if (req.params && req.params.firstId && req.params.secondId)
  {

    let first = req.params.firstId;
    let second = req.params.secondId;

    dal.createLink(first, second,
		   function (link) {res.status(200).send();},
		   function (err)  {res.status(500).send({ error: err });});
    
  }
  else
  {
    res.status(500).send({ error: 'wrong parameters' });
  }
});



module.exports = router;
