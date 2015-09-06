function post_login() {
    //$('#peers_page > .content').html('');
    $('#peers_page > .content > .row').html('');
    $('#peers_table').find('tr').html('<th>Address</th><th>Block height</th><th>Version</th><th>Latency</th><th>Used</th><th>Status</th><th>Rank</th>');
    $('#forging_indicator').remove(); 
    $('#nrs_version_info').html('UI Version');
    $('#nrs_version').parent().replaceWith('<span id="nrs_version2" class="small-box-footer"></span>');
    $('#nrs_version2').html('1.5.15');
    $('#add_peer_button').hide();
    
    NRS.pages.peers = function() {
        $.ajax({
            cache: false,
            url: '/api/getpeers',
            dataType: 'json',
            success: function(data) {
                var rows = '';
				for (var i = 0; i < data.result.length; i++) {
					var peer = data.result[i];
                    rows += '<tr><td>' + peer.address + '</td><td>' + peer.block_height + '</td><td>' + peer.version + '</td><td>' + peer.latency + '</td>';
                    rows += '<td>' + peer.connection_attempts + '</td><td>';
                    if (peer.consecutive_errors == 0)
                        rows += '<span class="label label-success">OK</span>';
                    else
                        rows += '<span class="label label-danger">Errors</span>';
                    rows += '</td><td>' + peer.rank + '</td></tr>';
                }
                NRS.dataLoaded(rows);
            }
        });
    }
}

function setNodemode(mode) {
    $.ajax({
        cache: false,
        url: '/api/setmode',
        dataType: 'json',
        data: "mode=" + mode,
        success: function(data) {}
    });
}

function addNode(address) {
    $.ajax({
        cache: false,
        url: '/api/addnode',
        dataType: 'json',
        data: 'address=' + address,
        success: function(data) {}
    });
}

function activateMultiGUI() {
    //inject ourselves after successful login (i.e. setupClipboardFunctionality)
    var original_function = NRS.setupClipboardFunctionality;
    NRS.setupClipboardFunctionality = function() {
        original_function.apply(this, arguments);    
        post_login();
    }
    
    //disable checkAliasVersions & checkIfOnAFork
    NRS.checkAliasVersions = function() {}
    NRS.checkIfOnAFork = function() {}
}

function choseMulti() {    
    setNodemode('multi');
    nxtlite_mode = 'Multi node';
    activateMultiGUI();
    bootbox.hideAll();
    ProceedWithInit();
}

function showAddNode() {
    //show input control
    $('#singlenode_add_ui').show();
    $('#singlenode_add_text').focus();
}

function addSingle() {
    //get the ip address
    var single_address = $('#singlenode_add_text').val();
    addNode(single_address);
    nxtlite_mode = 'Single node (' + single_address + ')';
    bootbox.hideAll();
    ProceedWithInit();
}

function showdialog() {
    var modalbody = '<div class="row">';
    modalbody += '<div class="col-md-12">';
    modalbody += 'In order to get started, please choose how you would like to access the Nxt network.';
    modalbody += '</div></div>';
    modalbody += '<div class="row" style="margin-top:25px;">';
    modalbody += '<div class="col-md-3 text-center">';
    modalbody += '<a href="javascript:choseMulti();" class="btn btn-primary btn-sm"><span class="glyphicon glyphicon-cloud"></span> Multi node</a>';
    modalbody += '</div><div class="col-md-9">';
    modalbody += 'If you are new to Nxt, this is what you want. NxtLite will use open nodes provided by the community to transact on Nxt.  Your secret key/phrase never leaves your local computer.';
    modalbody += '</div></div>';
    
    modalbody += '<div class="row"><div class="col-md-12"><hr/></div></div>';
        
    modalbody += '<div class="row" style="margin-top:15px;">';
    modalbody += '<div class="col-md-3 text-center">';
    modalbody += '<a href="javascript:showAddNode();" class="btn btn-primary btn-sm"><span class="glyphicon glyphicon-pushpin"></span> Single node</a>';
    modalbody += '</div><div class="col-md-9">';
    modalbody += 'Choose this if you run your own Nxt NRS node and wish to use it as your sole trusted access point to the Nxt network.';
    modalbody += '</div></div>';
    
    modalbody += '<div class="row" style="display:none;margin-top:10px;" id="singlenode_add_ui">';
    modalbody += '<div class="col-md-3 text-center">';
    modalbody += '</div><div class="col-md-9">';
    modalbody += '<b>IP Address / Hostname</b><br>';
    modalbody += '<input type="text" class="form-control" name="singlenode_add_text" id="singlenode_add_text">';
    modalbody += '<a href="javascript:addSingle();" style="margin-top:5px;" class="btn btn-primary">Add Node</a>';
    modalbody += '</div></div>';

    
    try {
        bootbox.dialog({
            'title': 'Welcome to NxtLite',
            'message': modalbody,
            'closeButton': false
        });
        //.find("div.modal-dialog").addClass("modal-lg");
    }
    catch (ex) {
        setTimeout(showdialog, 10);    
    }
}

function ProceedWithInit() {
    NRS.init();
    NRS.isLocalHost = false;
    $('#loading_msg').hide();

    var html = '<div style="max-width:400px;min-width:400px;display:inline-block;color:#ffffff;"><span style="font-size:20px;">NxtLite</span> ';
    html += '<div style="position:relative;top:-3px;" class="label label-default"><span class="glyphicon glyphicon-cog"></span> ' + nxtlite_mode + '</div>';
    html += '<button id="nxtlite_btn_reset" style="position:relative;top:-3px;margin-left:20px;" class="btn btn-default btn-xs" data-toggle="tooltip" data-placement="right" title="Reset NxtLite settings"><span class="glyphicon glyphicon glyphicon-refresh"></span></button>'; 
    html += '</div>';

    $('#login_panel').before(html);
    $('[data-toggle="tooltip"]').tooltip();
    
    //wire up the reset button
    $('#nxtlite_btn_reset').on('click', function() {
        $.ajax({
            cache: false,
            url: '/api/reset',
            dataType: 'json',
            success: function(data) {
                window.location.reload();
            }
        });
    });
}

function activateSingleGUI() {
	//hack to make Chrome redraw screen to fix scrollbar issue
    if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
        var original_function = NRS.setupClipboardFunctionality;
	    NRS.setupClipboardFunctionality = function() {
	        original_function.apply(this, arguments);    
	        $(window).trigger('resize');
	    }
	}
}

function getstatus() {
    //send ajax request for firstrun
    $.ajax({
            cache: false,
            url: '/api/getstatus',
            dataType: 'json',
            success: function(data) {
                //alert(data.result);
                if (data.result.mode == 'firstrun') {
                    showdialog();
                    return;
                }
                else if (data.result.mode == 'multi') {
                    nxtlite_mode = 'Multi node';
                    activateMultiGUI();
                }
                else if (data.result.mode == 'single') {
                    nxtlite_mode = 'Single node (' + data.result.address + ')';
                    activateSingleGUI();
                }
                
                ProceedWithInit();
            }
    });
}

//load bootbox.js
bootbox = $.getScript("bootbox.min.js");

//default some vars
var nxtlite_mode = '';

//check for first run
getstatus();