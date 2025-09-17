window.initializePainScale = function (dotNetHelper, initialValue) {
    $('#pain-scale-widget').painScale({
        selectedRating: initialValue, // Set initial value
        onFaceSelect: function (value) {
            dotNetHelper.invokeMethodAsync('UpdatePainValueFromJs', value);
        },
        onFaceClicked: function (value) {
            dotNetHelper.invokeMethodAsync('UpdateOnFaceSelected', value);
        },
    });
}

window.setPainScaleValue = function (value) {
    $('#pain-scale-widget').painScale('select', value);
}
