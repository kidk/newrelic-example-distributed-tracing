$(function() {

    //
    // Update todo list every second
    //
    function update() {
        // Retrieve latest todo list
        $.getJSON( "/todo/getList", function(data) {
            var items = [];
            $.each( data, function( key, val ) {
                if (val['isComplete']) {
                    items.push( "<li id='" + val['id'] + "'>+ " + val['name'] + "</li>" );
                } else {
                    items.push( "<li id='" + val['id'] + "'>x " + val['name'] + "</li>" );
                }
            });

            // Clear the old
            $('ul').empty();

            // Put in list
            $( "<ul/>", {
              html: items.join( "" )
            }).appendTo( "body" );
        });

        setTimeout(update, 1000);
    }
    update();

    //
    // Handle submit
    //
    $("#add").click(function( event ) {
        event.preventDefault();
        var action = $("#action").val();
        transfer(action);
    });

    //
    // Randomly send some data so we have a higher chance of getting a trace
    //
    function simulator() {
        messages = [
            "Take out the loundry",
            "Spend some time with the family",
            "Complain about corona on twitter",
            "Write distributed tracing example",
        ];

        var item = messages[Math.floor(Math.random() * messages.length)];
        transfer(item);

        setTimeout(simulator, 10000);
    }
    simulator();

    //
    // Send data to backend
    //
    function transfer(message) {
        console.log('sending following todo:', message);

        $.post("/todo/addItem",    {
            action: message,
        }, function(data, status) {
            console.log('data:', data, 'status:', status);
        });
    }
});
