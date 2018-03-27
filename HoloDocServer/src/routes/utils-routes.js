var express = require('express');
var router = express.Router();

// middleware that is specific to this router
router.use(function timeLog (req, res, next) {
  console.log('Time: ', Date.now());
  next();
});

router.post('/ping', function(req, res) {
	console.log('/ping');
	res.status(200).send({ Success: true });
});

module.exports = router;