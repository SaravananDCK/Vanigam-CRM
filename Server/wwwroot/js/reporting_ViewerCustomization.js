window.ReportingViewerCustomization = {
    //Change default Zoom level  
    onBeforeRender: function (s, e) {
        //-1: Page Width  
        //0: Whole Page  
        //1: 100%  
        e.reportPreview.zoom = -1;
        e.reportPreview.showMultipagePreview(true);
    }
}  