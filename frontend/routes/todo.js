var newrelic = require('newrelic');
var express = require('express');
var router = express.Router();
var request = require('request');
var azure = require('azure');

// set up azure service bus
var serviceBusService = azure.createServiceBusService(process.env.SERVICE_BUS);
serviceBusService.createQueueIfNotExists('todoqueue', function(error){
  if (!error){
    console.log('createQueueIfNotExists succeeded');
  } else {
    console.log('createQueueIfNotExists failed:', error)
  }
});

/* GET todo listing. */
router.get('/getList', function(req, res, next) {
  var options = {
    hostname: req.config.serviceBackendHost,
    port: req.config.serviceBackendPort,
    path: '/Todo/',
    method: 'GET',
    json: true,
    timeout: 100,
  }
  request(req.config.serviceBackend + '/Todo/', { json: true }, (err, response, body) => {
    if (err) {
      res.send('error:' + err);
      return console.log(err);
    }

    res.send(body);
  });
});

function preparePayload(payload) {
  // Call newrelic.getTransaction to retrieve a handle on the current transaction.
  const transaction = newrelic.getTransaction();
  var headerObject = {};
  transaction.insertDistributedTraceHeaders(headerObject);

  return {
    payload: payload,
    newrelic: headerObject,
  }
}

/* ADD todo listing. */
router.post('/addItem', function(req, res, next) {
  var action = req.body.action;

  // Push to service bus
  var message = {
    body: JSON.stringify(preparePayload({
      'action': action,
    })),
  };
  serviceBusService.sendQueueMessage('todoqueue', message, function(error){
    if (!error){
      console.log('sendQueueMessage succeeded');
    } else {
      console.log('sendQueueMessage failed:', error)
    }
  });

  res.send('done');
});

module.exports = router;
