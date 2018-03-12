var assert = require('assert');

describe('Testing unit testing - ', function() {


  beforeEach(function(done) {
    done();
  });

  afterEach(function(done) {
    done();
  });

  describe('Verifying that it is working:', function () {
    it('Assert false', function (done) {
      assert(false);
      done();
    });
    it('Assert True', function (done) {
      assert(true);
      done();
    });
  });

});
