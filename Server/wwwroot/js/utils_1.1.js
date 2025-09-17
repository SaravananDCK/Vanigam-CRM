// utils.js

window.getSubdomain = function () {
    // Get the full domain (e.g., "subdomain.example.com")
    var domain = window.location.hostname;

    // Split the domain by dots
    var parts = domain.split('.');

    // Subdomain is the first part if there are more than two parts
    if (parts.length > 2) {
        return parts[0];
    } else {
        return ""; // No subdomain
    }
};

function downloadFromByteArray(options) {
    var link = this.document.createElement('a');
    link.download = options.fileName;
    link.href = "data:" + options.contentType + ";base64," + options.byteArray;
    this.document.body.appendChild(link);
    link.click();
    this.document.body.removeChild(link);
}
function timeOutCall(dotnethelper) {
    document.onmousemove = resetTimeDelay;
    document.onkeypress = resetTimeDelay;

    function resetTimeDelay() {
        dotnethelper.invokeMethodAsync("TimerInterval");
    }
}
function scrollToBottom(elmName) {
    var height = document.getElementById(elmName).scrollHeight;
    //$(elmName).scrollTop($(elmName).height())
    $('#' + elmName).scrollTop(height)
}

// Click outside listener for dropdown menus
let dropdownInstances = [];

function addClickOutsideListener(dotnetReference) {
    dropdownInstances.push(dotnetReference);

    // Only add the global listener once
    if (dropdownInstances.length === 1) {
        document.addEventListener('click', function (event) {
            // Check if click is outside any dropdown
            const isClickInsideDropdown = event.target.closest('.modern-dropdown-wrapper') ||
                event.target.closest('.custom-dropdown-menu');

            if (!isClickInsideDropdown) {
                // Reset all button z-indexes
                const allActionButtons = document.querySelectorAll('.modern-action-btn');
                allActionButtons.forEach(btn => {
                    btn.style.zIndex = '10';
                });

                // Close all dropdowns
                dropdownInstances.forEach(instance => {
                    instance.invokeMethodAsync('CloseDropdowns');
                });
            }
        });
    }
}

function removeClickOutsideListener(dotnetReference) {
    const index = dropdownInstances.indexOf(dotnetReference);
    if (index > -1) {
        dropdownInstances.splice(index, 1);
    }
}

// Position dropdown relative to button
function positionDropdown(dropdownElementId, buttonElementId) {
    try {
        const dropdown = document.querySelector(`[data-ref="${dropdownElementId}"]`) ||
            document.getElementById(dropdownElementId) ||
            document.querySelector(`.custom-dropdown-menu`);

        const button = document.querySelector(`[data-ref="${buttonElementId}"]`) ||
            document.getElementById(buttonElementId) ||
            document.querySelector('.modern-action-btn');

        if (dropdown && button) {
            // Lower z-index of all other action buttons
            const allActionButtons = document.querySelectorAll('.modern-action-btn');
            allActionButtons.forEach(btn => {
                if (btn !== button) {
                    btn.style.zIndex = '5';
                }
            });

            // Ensure the current button has higher z-index than others but lower than dropdown
            button.style.zIndex = '100';

            // Ensure dropdown has the highest z-index
            dropdown.style.zIndex = '2147483647';

            const buttonRect = button.getBoundingClientRect();
            const dropdownRect = dropdown.getBoundingClientRect();

            // Calculate position
            const left = buttonRect.left + (buttonRect.width / 2);
            const top = buttonRect.bottom + 8;

            // Apply position
            dropdown.style.left = left + 'px';
            dropdown.style.top = top + 'px';
            dropdown.style.transform = 'translateX(-50%)';

            // Ensure dropdown is within viewport
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;

            // Adjust horizontal position if needed
            if (left - (dropdownRect.width / 2) < 10) {
                dropdown.style.left = '10px';
                dropdown.style.transform = 'translateX(0)';
            } else if (left + (dropdownRect.width / 2) > viewportWidth - 10) {
                dropdown.style.left = (viewportWidth - 10) + 'px';
                dropdown.style.transform = 'translateX(-100%)';
            }

            // Adjust vertical position if needed
            if (top + dropdownRect.height > viewportHeight - 10) {
                dropdown.style.top = (buttonRect.top - dropdownRect.height - 8) + 'px';
            }
        }
    } catch (error) {
        console.error('Error positioning dropdown:', error);
    }
}

// Reset all button z-indexes to default
function resetButtonZIndexes() {
    const allActionButtons = document.querySelectorAll('.modern-action-btn');
    allActionButtons.forEach(btn => {
        btn.style.zIndex = '10';
    });
}