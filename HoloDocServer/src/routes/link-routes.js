var express = require('express');
var router = express.Router();
const utils = require('./route-utils');
const dal = require('../database/dal.js');

// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

// Add a link between two documents
router.post('/create', function (req, res) {
  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    console.log(params)
    if (params && params.firstId && params.secondId)
    {
      let first = params.firstId;
      let second = params.secondId;

      dal.createLink(first, second,
  		   function (link) {res.status(200).send({ "Success": "youpi" });},
  		   function (err)  {res.status(500).send({ "Error": err });}
       );

    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });
});

// Add a link between two documents
router.post('/remove', function (req, res) {
  utils.asyncGetDataStream(req, function(buffer) {
    let params = JSON.parse(buffer);
    if (params && params.firstId)
    {
      let first = params.firstId;

      dal.deleteLink(first,
  		   function (link) {res.status(200).send({ "Success": "youpi" });},
  		   function (err)  {res.status(500).send({ "Error": err });});

    }
    else
    {
      res.status(500).send({ "Error": 'wrong parameters' });
    }
  });
});

module.exports = router;
