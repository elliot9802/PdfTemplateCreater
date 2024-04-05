function scrollToElementAndFocus(selector) {
    try {
        const element = document.querySelector(selector);
        if (element) {
            element.scrollIntoView({ behavior: "smooth", block: "nearest" });
            const input = element.querySelector("input");
            if (input) {
                input.focus();
            } else {
                console.error("Input element not found within:", element);
            }
        } else {
            console.error("Element not found for selector:", selector);
        }
    } catch (error) {
        console.error("scrollToElementAndFocus error:", error);
    }
}

function focusOnElementById(elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: "smooth", block: "nearest" });
            element.focus();
            console.log(`Focus set on element: ${elementId}`);
        } else {
            console.error(`Element not found for ID: ${elementId}`);
        }
    } catch (error) {
        console.error(`Error focusing on element by ID: ${elementId}`, error);
    }
}