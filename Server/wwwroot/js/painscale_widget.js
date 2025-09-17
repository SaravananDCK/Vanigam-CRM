(function ($) {
    $.widget("custom.painScale", {
        options: {
            faces: [
                { value: 0, description: 'No Hurt', unselectedImage: 'images/face-0.png', selectedImage: 'images/face-0-selected.png' },
                { value: 2, description: 'Hurts a Little Bit', unselectedImage: 'images/face-1.png', selectedImage: 'images/face-1-selected.png' },
                { value: 4, description: 'Hurts a Little More', unselectedImage: 'images/face-2.png', selectedImage: 'images/face-2-selected.png' },
                { value: 6, description: 'Hurts Even More', unselectedImage: 'images/face-3.png', selectedImage: 'images/face-3-selected.png' },
                { value: 8, description: 'Hurts a Whole Lot', unselectedImage: 'images/face-4.png', selectedImage: 'images/face-4-selected.png' },
                { value: 10, description: 'Hurts Worst', unselectedImage: 'images/face-5.png', selectedImage: 'images/face-5-selected.png' }
            ],
            selectedRating: null,
            onFaceSelect: null, // Callback when a face is selected
            onFaceClicked: null // Callback when a face is clicked
        },

        _create: function () {
            const self = this;
            const container = $('<div>', { class: 'scale' }).appendTo(this.element);

            this.options.faces.forEach(function (face) {
                const faceContainer = $('<div>', {
                    class: 'face-container',
                    'data-value': face.value,
                    click: function () {
                        self._selectFace(face.value);
                        self._clickFace(face.value);
                    },
                    mouseenter: function () {
                        $(this).find('img.face-image').attr('src', face.selectedImage);
                    },
                    mouseleave: function () {
                        const faceValue = $(this).data('value');
                        const faceData = self.options.faces.find(f => f.value === faceValue);
                        if (faceValue !== self.options.selectedRating) {
                            $(this).find('img.face-image').attr('src', faceData.unselectedImage);
                        }
                    }
                }).appendTo(container);

                $('<img>', {
                    src: face.unselectedImage,
                    alt: face.description,
                    class: 'face-image'
                }).appendTo(faceContainer);

                $('<h4>').text(face.value).appendTo(faceContainer);
                $('<h5>').text(face.description).appendTo(faceContainer);
            });

            this._updateSelected();
        },

        _selectFace: function (value) {
            this.options.selectedRating = value;
            this._updateSelected();

            if (typeof this.options.onFaceSelect === "function") {
                this.options.onFaceSelect(value);
            }
        },
        _clickFace: function (value) {
            if (typeof this.options.onFaceClicked === "function") {
                this.options.onFaceClicked(value);
            }
        },

        _updateSelected: function () {
            const self = this;

            this.element.find('.face-container').each(function () {
                const faceValue = $(this).data('value');
                const faceData = self.options.faces.find(f => f.value === faceValue);
                const imageElement = $(this).find('img.face-image');

                if (faceValue === self.options.selectedRating) {
                    imageElement.attr('src', faceData.selectedImage);
                } else {
                    imageElement.attr('src', faceData.unselectedImage);
                }
            });

            //const ratingText = this.options.faces[this.options.selectedRating]?.description || 'None';
            const selectedFace = this.options.faces.find(face => face.value === this.options.selectedRating);
            const ratingText = selectedFace ? selectedFace.description : 'None';
            $('#selected-rating').text(`Selected Pain Level: ${this.options.selectedRating} - ${ratingText}`);
        },

        select: function (value) {
            this._selectFace(value);
        },

        clear: function () {
            this.options.selectedRating = null;
            this._updateSelected();
        }
    });

    // Initialize the widget when the page loads
    $(function () {
        $('#pain-scale-widget').painScale();
    });
})(jQuery);
