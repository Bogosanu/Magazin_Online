
    document.addEventListener("DOMContentLoaded", function() {
    var stars = document.querySelectorAll('.star-rating .star');

    stars.forEach(function(star, index) {
        star.addEventListener('click', function () {
            
            var rating = this.getAttribute('data-value');

            
            document.querySelector('input[name="Points"]').value = rating;


            
            stars.forEach(function (s, idx) {
                if (idx < rating) {
                    s.innerHTML = '<i class="fas fa-star"></i>'; // filled star
                } else {
                    s.innerHTML = '<i class="far fa-star"></i>'; // empty star
                }
            });
        });
    });
});

