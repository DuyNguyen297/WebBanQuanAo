function changeImage(element) {
    // Remove the zoom-in class to reset the animation
    const animatedImage = $('#main_product_image');
    animatedImage.removeClass('zoom-in');

    // Set the new source after a short delay (to allow the reset)
    setTimeout(function () {
        animatedImage.attr('src', element.src);

        // Add the zoom-in class to trigger the animation again
        animatedImage.addClass('zoom-in');
    }, 100);
}

let items = document.querySelectorAll('.carousel .carousel-item')

items.forEach((el) => {
    const minPerSlide = 4
    let next = el.nextElementSibling
    for (var i = 1; i < minPerSlide; i++) {
        if (!next) {
            // wrap carousel by using first child
            next = items[0]
        }
        let cloneChild = next.cloneNode(true)
        el.appendChild(cloneChild.children[0])
        next = next.nextElementSibling
    }
})