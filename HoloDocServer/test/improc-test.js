var assert = require('assert');

const cv = require('opencv4nodejs');
const utils = require('../improc/improc-utils.js');
const improc = require('../improc/improc.js');
const reco = require('../improc/improc-recognition.js');


describe('Testing Image Processing ', function() {
  this.timeout(5000);
  let im1, im2, im3, im4, im5;

  beforeEach(function(done) {
    im1 = cv.imread('./test/res/1.jpg');
    im2 = cv.imread('./test/res/2.jpg');
    im3 = undefined;
    im4 = cv.imread('./test/res/3.png');
    im5 = cv.imread('./test/res/4.png');
    done();
  });

  afterEach(function(done) {
      im1 = undefined;
      im2 = undefined;
      im3 = undefined;
      im4 = undefined;
      im5 = undefined;
      done();
    });

    describe('Testing Document Detection', function () {

      it('Document Detected on good image', function (done) {
        let Docs = improc.detectDocuments(im1);
        assert(Docs.length == 2);
        assert(Docs[0].length == 4 && Docs[1].length == 4);
        done();
      });

      it('Document Detected on false image', function (done) {
        let Docs = improc.detectDocuments(im2);
        assert(Docs.length == 0);
        done();
      });

      it('Document Detected on no image', function (done) {
        let Docs = improc.detectDocuments(im3);
        assert(Docs.length == 0);
        done();
      });

    });

    describe('Testing Document Extraction', function () {

      it('Document Extraction on good image', function (done) {
        let Docs = improc.detectDocuments(im1);
        let center = utils.getCenter(im1);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im1, toExtract);

        assert(croped);
        done();
      });

      it('Document Extraction on false image', function (done) {
        let Docs = improc.detectDocuments(im2);
        let center = utils.getCenter(im2);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im2, toExtract);

        assert(!croped);
        done();
      });

      it('Document Extraction on no image', function (done) {
        let Docs = improc.detectDocuments(im3);
        let center = utils.getCenter(im3);
        let toExtract = utils.getNearestdocFrom(Docs, center);
        let croped = improc.undistordDoc(im3, toExtract);

        assert(!croped);
        done();
      });

    });

    describe('Testing Document Recognition', function () {

      describe('Testing Feature Extraction', function () {
        it('Feature extraction with a correct image input', function (done) {
          let features = reco.extractFeatures(im4);

          assert(features);
          assert(features.length == 6);
          assert(features.every(function (x) {return x.length == reco.HIST_BINS}));
          done();
        });

        it('Feature extraction with a correct image input and a given number of bins', function (done) {
          let bins = 42;
          let features = reco.extractFeatures(im4, bins);

          assert(features);
          assert(features.length == 6);
          assert(features.every(function (x) {return x.length == 42}));
          done();
        });

        it('Feature extraction with an invalid image', function (done) {
          let features = reco.extractFeatures(undefined);

          assert(!features);
          done();
        });

      });

      describe('Testing features distance function', function () {
        it('Distance between the same image', function (done) {
          let features1 = reco.extractFeatures(im4);
          let features2 = reco.extractFeatures(im4);

          let dist = reco.featuresDistance(features1, features2);

          assert(features1);
          assert(features2);
          assert(dist != undefined);
          assert(dist < Number.EPSILON);

          assert (reco.featureDistanceNormalization(dist) < Number.EPSILON)
          done();
        });

        it('Distance between oppisites images', function (done) {
          let features1 = reco.extractFeatures(im4);
          let features2 = reco.extractFeatures(im5);

          let dist = reco.featuresDistance(features1, features2);

          assert(features1);
          assert(features2);
          assert(dist != undefined);
          assert(dist > reco.MAX_FEATURES_DISTANCE - Number.EPSILON);

          assert (reco.featureDistanceNormalization(dist) > 1 - Number.EPSILON)
          done();
        });
      });

    });
});
