function renderjQueryComponents(dotNetHelperRef, sliderName, min, max, step, val1, val2, val3, val4) {
    $('#' + sliderName).createSlider(dotNetHelperRef,min, max, step, val1, val2, val3, val4);
}
$(function () {

    // function to create slider ticks
    var setSliderTicks = function () {
        // slider element
        var $slider = $('.slider');
        var max = $slider.slider("option", "max");
        var min = $slider.slider("option", "min");
        var step = $slider.slider("option", "step");
        var spacing = 100 / (max - min);
        // tick element
        var ticks = $slider.find('div.ticks');

        // remove all ticks if they exist
        $slider.find('.ui-slider-tick-mark-main').remove();
        $slider.find('.ui-slider-tick-mark-text').remove();
        $slider.find('.ui-slider-tick-mark-side').remove();

        // generate ticks
        for (var i = min; i <= max; i = i + step) {

            // main ticks (whole number)
            if (i % 1 === 0) {
                $('<span class="ui-slider-tick-mark-main"></span>').css('left', (spacing * i) + '%').appendTo(ticks);
                $('<span class="ui-slider-tick-mark-text">' + i + '</span>').css('left', (spacing * i) + '%').appendTo(ticks);
            }
            // side ticks
            else {
                $('<span class="ui-slider-tick-mark-side"></span>').css('left', (spacing * i) + '%').appendTo(ticks);
            }
        }
    };
    $.fn.createSlider = function (dotNetHelperRef,minVal,maxVal,stepVal,val1,val2,val3,val4) {
        // create slider
        $(this).slider({
            // set min and maximum values
            // day hours in this example
            min: minVal,
            max: maxVal,
            dotNetHelper: dotNetHelperRef,
            // step
            // quarter of an hour in this example
            step: stepVal,
            // range
            range: false,
            // show tooltips
            tooltips: true,
            // current data
            handles: [{
                value: val1,
                type: "abnormalLow"
            }, {
                value: val2,
                type: "normal"
            }, {
                value: val3,
                type: "abnormalHigh"
            }, {
                value: val4,
                type: "criticalHigh"
            }],
            // display type names
            showTypeNames: true,
            typeNames: {
                'abnormalLow': 'Ab-Normal Low',
                'normal': 'Normal',
                'abnormalHigh': 'Ab-Normal High',
                'criticalHigh': 'Critical High'
            },
            // main css class (of unset data)
            mainClass: 'criticalHigh',
            // slide callback
            slide: function (e, ui) {
                //console.log(e, ui);
            },
            stop: function (e, ui) {
                //console.log('stop',e, ui);
                //console.log(ui.value, ui.values.indexOf(ui.value));
                //console.log(e);
                //dotNetHelper.invokeMethod('UpdateValue', ui.value, ui.values.indexOf(ui.value))
            },
            // handle clicked callback
            handleActivated: function (event, handle) {
                // get select element
                var select = $(this).parent().find('.slider-controller select');
                // set selected option
                select.val(handle.type);
            }

        });
    };

    // when clicking on handler
    $(document).on('click', '.slider a', function () {
        var select = $('.slider-controller select');
        // enable if disabled
        //select.attr('disabled', false);
        alert($(this).attr('data-type'));
        select.val($(this).attr('data-type'));
        /*if ($(this).parent().find('a.ui-state-active').length)
          $(this).toggleClass('ui-state-active');*/
    });
});