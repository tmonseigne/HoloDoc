var express = require('express');
var router = express.Router();

// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});


// define the home page route
// Will return the web application
router.get('/',function (req, res) {
  res.send('<p>Hey buddy !</p>');
});

module.exports = router;
