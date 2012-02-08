var zocMon = {

    flots: [],
    flotOpts: [],
    flotCurrentOpts: [],
    flotDataSources: [],
    flotLabels: [],
    flotData: [],

    ONE_DAY: 86400000,
    calendarOpts: {
        usa: true,
        isoTime: true
    },

    graphItem: null,
    FLOT_LABEL_Y_OFFSET: 14,
    savedGroup: null,
    removeFromEdit: { graphCount: '', password: '' },
    GRAPHS_BASE_URL: '/remote/infrastructure/Graphs?name=&password=Red007Flag',
    reduceLevelToHistoryRange: {
        "MinutelyData": "LastHour",
        "FiveMinutelyData": "LastFourHours",
        "HourlyData": "LastWeek",
        "DailyData": "LastMonth"
    },

    reduceLevels: ['MinutelyData', 'FiveMinutelyData', 'HourlyData', 'DailyData'],

    previousPoint: null,

    refreshUrl: window.location.href,


    initZocMon: function () {

        var dates = $('#fromDate, #toDate').datepicker({
            dateFormat: 'm/dd/yy',
            onSelect: function(selectedDate) {
                var option = this.id == "fromDate" ? "minDate" : "maxDate",
                        instance = $(this).data("datepicker"),
                        date = $.datepicker.parseDate(
                                instance.settings.dateFormat ||
                                        $.datepicker._defaults.dateFormat,
                                selectedDate, instance.settings);
                $('#historyRange').val(0).siblings('.-select-text').html('Select...');
                dates.not(this).datepicker("option", option, date);
            }
        }),
        graphBankForm = $('#graphBankForm'),
        addForm = $('#addForm'), date = new Date();

        //set default dates
        dates.filter('#toDate').datepicker('option', 'defaultDate', null)
                .datepicker('setDate', date);
        dates.filter('#fromDate').datepicker('option', 'defaultDate', -1)
                .datepicker('setDate', new Date(date.getTime() - zocMon.ONE_DAY));


        //init general stuff
        this.graphItem = _.template($('#graphItem').html());

        $('.savedGroupsInner #formAction').val('go');
        graphBankForm
                .attr('method', 'get')
                .attr('action', '/remote/infrastructure/Graphs');

        addForm.find("#fromTime, #toTime").timepicker({
            ampm: true,
            timeFormat: 'h:mm TT'
        }).val('12:00 AM');

        $('#filter').quicksearch('.configOption', {
            'delay': 100,
            'noResults': '#noResults',
            'bind': 'keyup keydown',
            'prepareQuery': function (val) {
                return new RegExp(val, "i");
            },
            'testQuery': function (query, txt, _row) {
                return query.test(txt);
            }
        }).val('');


        //init events
        addForm.find('.resetLink').click(function() {
            //full reset of forms
            $('#addForm')[0].reset();
            $(':input', '#configOptionForm')
                    .not(':button, :submit, :reset, :hidden')
                    .val('')
                    .removeAttr('checked')
                    .removeAttr('selected');
            $('#filter').val('');
            zocMon.checkLineRatio($('input[name="graphType"]:checked'));
        });

        addForm.find('input[name="graphType"]').click(function(){
            zocMon.checkLineRatio($(this));
        });

        zocMon.checkLineRatio($('input[name="graphType"]:checked'));

        $('.addLink').click(function() {
            var addForm = $('#addForm'),
                formArr = addForm.serializeArray(),
                dataSources = $('#configOptionForm').serializeArray(),
                graphItem = $('<div></div>').addClass('graphItem'),
                reduceLevel = $('.reduceLevel input[name="reduceLevel"]:checked'),
                title = $('#titleBox').val(),
                graphType = $('input[name="graphType"]:checked');

            if (dataSources.length == 0) {
                alert('You must choose at least one Data Source');
                return;
            }

            if (reduceLevel.length == 0) {
                alert('You must choose a Reduce level');
                return;
            }

            if (graphType.length == 0) {
                alert('You must choose a graph type');
                return;
            }

            $('.graphBank').append(zocMon.graphItem(
                {
                    title: title,
                    formArr: formArr,
                    dataSources: dataSources,
                    reduceLevel: reduceLevel
                }
            ));

            //execute partial reset of form:
            //reset reduce levels, other options, title, and datasources
            //maintain history range / from and to dates
            $('#configOptionForm input[type="checkbox"]')
                    .removeAttr('checked');
            $('#filter').val('').trigger('keyup');
            addForm.find('#titleBox').val('');
            addForm.find('.reduceLevel input, .otherOptions input')
                    .not('#minutelyData, #showYAxis, #lineGraph')
                    .removeAttr('checked');
            addForm.find('#minutelyData, #showYAxis').attr('checked', 'checked');
            addForm.find('#lineGraph')
                    .attr('checked', 'checked');
            zocMon.checkLineRatio($('input[name="graphType"]:checked'));

        });

        graphBankForm.find('.generateLink').click(function() {
            if (zocMon.prepGraphBank()){
                var graphBankForm = $('#graphBankForm');
                graphBankForm
                        .attr('method', 'get')
                        .attr('target', '_blank')
                        .attr('action', '/remote/infrastructure/Graphs')
                        .submit();
            }
        });

        graphBankForm.find('.saveLink').click(function() {
            var name = $.trim($('#name').val());
            if (name == '') {
                alert('You have to name your group of graphs in order to save.');
                return;
            }
            if (zocMon.prepGraphBank()){
                $('#graphBankForm')
                        .find('#graphBankFormAction').val('saveNew').end()
                        .attr('method', 'post')
                        .attr('target', '_self')
                        .attr('action', '/remote/infrastructure/ZocMonGraph?password=Red007Flag')
                        .submit();
            }
        });

        $('.goLink').click(function() {
            $(this).siblings('#formAction').val('go');
            $(this).parents('form').attr('method', 'get')
                    .attr('target', '_blank')
                    .attr('action', '/remote/infrastructure/Graphs')
                    .submit();
        });

        $('.deleteLink').click(function() {
            if (confirm('Delete this saved group?')) {
                $(this).parent().siblings('#formAction').val('delete');
                $(this).parents('form').attr('method', 'post')
                        .attr('target', '_self')
                        .attr('action', '/remote/infrastructure/ZocMonGraph?password=Red007Flag')
                        .submit();
            }
        });

        $('.editLink').click(function() {
            $(this).parent().siblings('#formAction').val('edit');
            $(this).parents('form').attr('method', 'post')
                    .attr('target', '_self')
                    .attr('action', '/remote/infrastructure/ZocMonGraph?password=Red007Flag')
                    .submit();
        });

        $('.savedGroupsInner .showMore').toggle(function(){
            $(this)
                .html('show fewer options')
                .siblings('.otherSavedGroupOptions').show();
        }, function(){
            $(this)
                .html('show more options')
                .siblings('.otherSavedGroupOptions').hide();
        });

        graphBankForm.delegate('.removeLink', 'click', function() {
            $(this).parent().remove();
        });

        $('.selectAllLink').click(function() {
            var visible = $('.configResults input[type="checkbox"]:visible');
            if (visible.length) {
                if (visible.filter(':checked').length) {
                    visible.removeAttr('checked');
                }
                else {
                    visible.attr('checked', 'checked');
                }
            }
        });

        $('.clearLink').click(function() {
            $(this).siblings(".graphItem").remove();
        });
//
//        $('.showHideFromTo').toggle(function() {
//            $(this).html('Hide "From" and "To" options').siblings('.fromToArea').show();
//            $('.vertDivider').css('height', 514);
//        }, function() {
//            $(this).html('Show "From" and "To" options').siblings('.fromToArea').hide();
//            $('.vertDivider').css('height', 455);
//        });

        $('.reduceLevel input[name="reduceLevel"]').change(function() {
            zocMon.checkReduceLevel();
        });
        zocMon.checkReduceLevel();

        //widget initializers
        $('form').form();
        $('.placeholder').placeholder();
        $('.button').button();
        $('select').select();

        if (this.savedGroup != null){
            this.initEditGroup();
        }
    },

    checkLineRatio: function(clicked){
        if (clicked.val() == 'Ratio') {
            $('.showEvents, .reduceLevel, .historyRange').css('visibility', 'hidden');
        }
        else if (clicked.val() == 'Line'){
            $('.showEvents, .reduceLevel, .historyRange').css('visibility', 'visible');
        }
    },

    checkReduceLevel: function(){
        var checked = $('.reduceLevel input[name="reduceLevel"]:checked'),
            numChecked = checked.length,
            currentVal;

        if (numChecked > 0) {
            currentVal = checked.eq(numChecked - 1).val();
            if (zocMon.reduceLevelToHistoryRange[currentVal]) {
                $('#historyRange').val(zocMon.reduceLevelToHistoryRange[currentVal])
                        .siblings('.-select-text').html(zocMon.reduceLevelToHistoryRange[currentVal]);
            }
        }
    },

    initEditGroup: function(){
        var urlArr = this.savedGroup.query.split('&'),
            i, max, tempArr,
            finalArr = [], finalIndex,
            graphBank = $('.graphBank');

        //via: http://ntt.cc/2008/05/10/over-10-useful-javascript-regular-expression-functions-to-improve-your-web-applications-efficiency.html
        function getdigits (s) {
           return s.replace (/[^\d]/g, '');
        }

        for ( i = 0, max = urlArr.length; i < max; i++ ) {
            tempArr = urlArr[i].split('=');

            if (tempArr.length != 2){
                continue;
            }

            finalIndex = getdigits(tempArr[0]);
            if (finalIndex != ''){
                finalIndex = parseInt(finalIndex);
            }

            if (!finalArr[finalIndex]){
                finalArr[finalIndex] = {
                    title: '',
                    formArr: [],
                    dataSources: [],
                    reduceLevel: []
                };
            }

            tempArr[0] = tempArr[0].replace(/[0-9]/g, '');

            if (!this.removeFromEdit[tempArr[0]]){

                if (tempArr[0] == 'reduceLevel'){
                    $.each(tempArr[1].split(','), function(index, value){
                        var reduceLevelArr = value.split('/');

                        finalArr[finalIndex].dataSources.push({
                            name: reduceLevelArr[0]
                        });

                        if (index == 0){
                            finalArr[finalIndex].reduceLevel.push({
                                value: reduceLevelArr[1].replace(reduceLevelArr[0], '')
                            });
                        }
                    });
                }
                else  if (tempArr[0] == 'titleBox'){
                    finalArr[finalIndex].title = tempArr[1];
                }
                else {
                    finalArr[finalIndex].formArr.push({
                        name: tempArr[0],
                        value: tempArr[1]
                    });
                }
            }
        }

        for (i = 0, max = finalArr.length; i < max; i++) {
            graphBank.append(zocMon.graphItem(finalArr[i]));
        }

        graphBank.find('.saveEditsLink').click(function(){
            if (zocMon.prepGraphBank()){
                $('#graphBankForm')
                        .find('#graphBankFormAction').val('saveEdits').end()
                        .find('#name').removeAttr('disabled').end()
                        .attr('method', 'post')
                        .attr('target', '_self')
                        .attr('action', '/remote/infrastructure/ZocMonGraph?password=Red007Flag')
                        .submit();
            }
        });

        graphBank.find('.cancelEditsLink').click(function(){
            graphBank
                .find('.clearLink').click().end()
                .find('#name').val('').removeAttr('disabled').end()
                .find('.saveLink, .generateLink').show().end()
                .find('.saveEditsLink, .cancelEditsLink').hide();
        });

        graphBank
            .find('#name').val(zocMon.savedGroup.name).attr('disabled','disabled').end()
            .find('#graphBankSavedGroupId').val(zocMon.savedGroup.id).end()
            .find('.saveLink, .generateLink').hide().end()
            .find('.saveEditsLink, .cancelEditsLink').show();
    },

    prepGraphBank: function() {
        var graphBank = $('.graphBank'),
            items = graphBank.find('.graphItem'),
            count = items.length,
            graphCountInput;

        if (count > 0) {
            graphCountInput = graphBank.find('input[name="graphCount"]');
            if (graphCountInput.length > 0) {
                graphCountInput.val(count);
            } else {
                graphBank.append($('<input type="hidden" />')
                        .attr('name', 'graphCount')
                        .attr('value', count));
            }

            items.each(function(index, value) {
                $(value).find('input').each(function() {
                    $(this).attr('name', $(this).attr('data-origName') + index);
                });
            });
        }
        else {
            alert('Add a graph to continue.');
            return false;
        }
        return true;
    },

    initGraphsPage: function() {
        if (zocMonGraphs) {
            this.makeGraphs(zocMonGraphs);
        }

        $('.toggleServerEvents').click(function(){
            var index = $(this).attr('data-index');
            if ($(this).attr('data-showevents') == 'true'){
                zocMon.redrawWithChanges(index, {
                    grid: {
                        markings: null
                    }
                });
                $('#placeholder' + index).attr('data-showevents', 'false');
                $(this).attr('data-showevents', 'false');
            }
            else {
                zocMon.redrawWithChanges(index, zocMon.flotOpts[index]);
                zocMon.drawLabels(index);
                $('#placeholder' + index).attr('data-showevents', 'true');
                $(this).attr('data-showevents', 'true');
            }

            zocMon.updateRefreshUrl();
        });

        $('.clearZoom').click(function(){
            var paramObj = { xaxis: { min: null, max: null }},
                    index = $(this).attr('data-index');

            if ($(this).parents('.graphWrapper').hasClass('MonthReview')){
                paramObj = zocMon.flotOpts[index];
            }

            zocMon.redrawWithChanges(index, paramObj);

            if ($('#placeholder' + index).attr('data-showevents') == 'true'){
                zocMon.drawLabels(index);
            }

            zocMon.updateRefreshUrl();
        });
    },

    makeGraphs: function(arrOpts) {
        var opts, i, max,
            defaultOpts = {
                xaxis: {
                    mode: "time",
                    timeformat: "%y-%0m-%0d %H:%M:%S"
                },
                yaxis: {
                    show: true,
                    tickFormatter: function (v) {
                        return v;
                    },
                    min: null
                },
                legend: {
                    noColumns: 2,
                    container: '#legend'
                },
                grid: {
                    hoverable: true,
                    clickable: true
                }
            },
            placeholder, labels;

        for (i = 0, max = arrOpts.length; i < max; i++) {

            opts = $.extend(true, {}, defaultOpts, arrOpts[i].opts);
            labels = arrOpts[i].labels;

            if (arrOpts[i].alertStatus) {
                opts.grid.backgroundColor = {
                    colors: ["#fff", "#eee"]
                }
            }

            if (arrOpts[i].isRatio) {
                opts.bars = {
                    show: true,
                    barWidth: .98,
                    fill: 0.75
                };

                opts.xaxis = arrOpts[i].xaxis;
            }

            if (arrOpts[i].type && arrOpts[i].type == "Line") {
                opts.selection = { mode: "x" };
                if (max > 6) {
                    opts.xaxis.show = false;
                }
            }

            if (arrOpts[i].type && arrOpts[i].type == "MonthReview") {
                var date = new Date();
                opts.xaxes = [
                    $.extend(true, {}, opts.xaxis, {
                        min: date.getTime() - 28*zocMon.ONE_DAY,
                        max: date.getTime() - 21*zocMon.ONE_DAY,
                        tickColor: 0
                    }),
                    $.extend(true, {}, opts.xaxis, {
                        min: date.getTime() - 21*zocMon.ONE_DAY,
                        max: date.getTime() - 14*zocMon.ONE_DAY,
                        tickColor: 1
                    }),
                    $.extend(true, {}, opts.xaxis, {
                        min: date.getTime() - 14*zocMon.ONE_DAY,
                        max: date.getTime() - 7*zocMon.ONE_DAY,
                        tickColor: 2
                    }),
                    $.extend(true, {}, opts.xaxis, {
                        min: date.getTime() - 7*zocMon.ONE_DAY,
                        max: date.getTime(),
                        tickColor: 3
                    })
                ];
                delete opts.xaxis;
                opts.selection = { mode: "x" };
                for (var j = 0, maxj = arrOpts[j].labeledData.length; j< maxj; j++){
                    arrOpts[i].labeledData[j].xaxis = j + 1;
                }
            }

            if (arrOpts[i].labeledData.length > 6 ){
                opts.legend.noColumns = 3;
            }

            opts.legend.container = '#legend' + arrOpts[i].index;
            placeholder = $("#placeholder" + arrOpts[i].index);

            //store opts and data for redrawing later
            this.flotOpts[arrOpts[i].index] = opts;
            this.flotCurrentOpts[arrOpts[i].index] = opts;
            this.flotDataSources[arrOpts[i].index] = arrOpts[i].dataSources;
            this.flotLabels[arrOpts[i].index] = labels;
            this.flotData[arrOpts[i].index] = arrOpts[i].labeledData;

            //create flot
            this.flots[arrOpts[i].index] = $.plot(placeholder, arrOpts[i].labeledData, opts);

            if (!arrOpts[i].isRatio) {

                if (arrOpts[i].showEvents){
                    zocMon.drawLabels(arrOpts[i].index);
                }
                else {
                    zocMon.redrawWithChanges(arrOpts[i].index, {
                        grid: {
                            markings: null
                        }
                    });
                }

                placeholder.bind("plothover", function (event, pos, item) {
                    if (item) {
                        if (zocMon.previousPoint != item.dataIndex) {
                            zocMon.previousPoint = item.dataIndex;

                            $("#tooltip").remove();
                            var x = item.datapoint[0],
                                y = (item.datapoint[1] > 1) ? item.datapoint[1].toFixed(2) :  item.datapoint[1].toFixed(5),
                                    date = new Date(x + 18000000);

                            zocMon.showTooltip("x = " + date.getMonth() + '/' + date.getDate() + " " + date.toTimeString().substr(0, 8) + "<br/>" +
                                    "y = " + y + "<br/>" +
                                    item.series.label, item);
                        }
                    }
                    else {
                        $("#tooltip").remove();
                        zocMon.previousPoint = null;
                    }
                });

                placeholder.bind('plotselected', (function() {
                    var index = arrOpts[i].index;
                    return function(event, ranges) {
                            var paramObj =  {
                                xaxis: {
                                    min: ranges.xaxis.from,
                                    max: ranges.xaxis.to
                                }
                            };
                            if ($(this).parents('.graphWrapper').hasClass('MonthReview')){
                                paramObj = {
                                    xaxes: [
                                        {
                                            min: ranges.xaxis.from,
                                            max: ranges.xaxis.to
                                        },
                                        {
                                            min: ranges.x2axis.from,
                                            max: ranges.x2axis.to
                                        },
                                        {
                                            min: ranges.x3axis.from,
                                            max: ranges.x3axis.to
                                        },
                                        {
                                            min: ranges.x4axis.from,
                                            max: ranges.x4axis.to
                                        }
                                    ]
                                };
                            }

                            zocMon.redrawWithChanges(index, paramObj);

                            if ($('#placeholder' + index).attr('data-showevents') == 'true'){
                                zocMon.drawLabels(index);
                            }
                        };
                    })()
                );

            }
            else {
                placeholder.bind('plotclick', function(event, pos, item){
                    if (item) {
                        var dataSource = zocMon.flotDataSources[$(event.target).attr('data-index')][item.series.data[0][0] - .5],
                            url = zocMon.GRAPHS_BASE_URL +
                                "&graphCount=1" +
                                "&isMonthReview=true" +
                                "&ReduceLevel0=";

                        for (var i = 0, max = zocMon.reduceLevels.length; i < max; i++) {
                            dataSource = dataSource.replace(zocMon.reduceLevels[i], '');
                        }

                        window.open(url + dataSource);
                    }
                });

                placeholder.bind("plothover", function (event, pos, item) {
                    if (item) {
                        if (zocMon.previousPoint != item.dataIndex) {
                            zocMon.previousPoint = item.dataIndex;

                            $("#tooltip").remove();
                            var x = item.datapoint[0],
                                y = item.datapoint[1].toFixed(2),
                                    date = new Date(x);

                            zocMon.showTooltip("ratio = " + y + "<br/>" +
                                    item.series.label, item);

                        }
                    }
                    else {
                        $("#tooltip").remove();
                        zocMon.previousPoint = null;
                    }
                });
            }
        }
    },

    redrawWithChanges: function(index, changes){
        this.flotCurrentOpts[index] = $.extend(true, {}, this.flotCurrentOpts[index], changes);
        this.flots[index] = $.plot($("#placeholder" + index),
                this.flotData[index],
                $.extend(true, {}, this.flotCurrentOpts[index], changes));
    },

    drawLabels: function(index){
        var labels = this.flotLabels[index],
            j, maxj, offset, yOffsets = {}, usableOffset, placeholder = $("#placeholder" + index);

        for (j = 0, maxj = labels.length; j < maxj; j++) {
            offset = this.flots[index].pointOffset({ x: labels[j].x, y: labels[j].y});
            usableOffset = labels[j].x.toString().substring(0, 6);
            if (yOffsets[usableOffset] != null) {
                yOffsets[usableOffset]++;
            }
            else {
                yOffsets[usableOffset] = 1;
            }

            // we just append it to the placeholder which Flot already uses
            // for positioning
            if (offset.left > 0 && offset.left < placeholder.width()) {
                placeholder.append('<div style="position:absolute;left:' +
                        (offset.left + 4) + 'px;top:' + (this.FLOT_LABEL_Y_OFFSET * yOffsets[usableOffset]) + 'px;color:#666;font-size:smaller">'
                        + labels[j].text + '</div>');
            }
        }
    },

    showTooltip: function(contents, item) {
        var cssObj = {
            position: 'fixed',
            display: 'none',
            top: 0,
            border: '1px solid #fdd',
            padding: '8px',
            'background-color': '#fee',
            opacity: 0.80,
            'font-size': '26px',
            'line-height': '34px'
        };

        if (item.pageX > window.innerWidth - 300){
            cssObj.left = 0;
        }
        else {
            cssObj.right = 0;
        }

        $('<div id="tooltip">' + contents + '</div>').css(cssObj).appendTo("body").fadeIn(200);
    },

    tryRefresh: function() {
        var url = "/remote/infrastructure/ZocMonGraph?password=Red007Flag";

        $.ajax({
            type: "GET",
            url: url,
            success: function() {
                $(".refreshResult").html("Success!");
                setTimeout(function() {
                    var url = window.location.href;

                    //first remove fields we will be replacing from the url string
                    url = url.replace(/&showevents(\d|\d\d)=(true|false|on|off)/g,'');

                    $('.toggleServerEvents').each(function(){
                        url += '&showevents' + $(this).attr('data-index') + '=' + $(this).attr('data-showevents');
                    });

                    window.location.href = url;
                }, 2000);
            },
            error: function() {
                $('.refreshResult').html('<h2>last update failed at: ' + new Date() + '</h2>');
                $('.timeStamp').css('color', 'red');
                setTimeout(zocMon.tryRefresh, 60000);
            }
        });
    },

    updateRefreshUrl: function(){
        var url = this.refreshUrl;
        url = url.replace(/&showevents(\d|\d\d)=(true|false|on|off)/g,'');
        $('.toggleServerEvents').each(function(){
            url += '&showevents' + $(this).attr('data-index') + '=' + $(this).attr('data-showevents');
        });
        this.refreshUrl = url;
    },

    dateToString: function(date){
        return date.getUTCMonth() + '/' + date.getUTCDay() + '/' + date.getUTCFullYear();
    }
};