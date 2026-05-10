// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", () => {
    const searchForm = document.getElementById("publicSearchForm");
    const resultsContainer = document.getElementById("publicResults");

    const submitSearch = () => {
        const action = searchForm?.getAttribute("action") || window.location.pathname;
        const formData = new FormData(searchForm);
        const url = new URL(action, window.location.origin);
        url.search = new URLSearchParams(formData).toString();

        if (!resultsContainer) {
            window.location.href = url.toString();
            return;
        }

        fetch(url.toString(), {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then((response) => response.text())
            .then((html) => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, "text/html");
                const updatedResults = doc.getElementById("publicResults");

                if (updatedResults) {
                    resultsContainer.innerHTML = updatedResults.innerHTML;
                    history.replaceState(null, "", url.toString());
                } else {
                    window.location.href = url.toString();
                }
            })
            .catch(() => {
                window.location.href = url.toString();
            });
    };

    if (searchForm) {
        const searchInput = searchForm.querySelector(".search-bar");
        const filterSelects = searchForm.querySelectorAll(".filter-select");
        let timerId;
        const debounceMs = 400;

        searchForm.addEventListener("submit", (event) => {
            event.preventDefault();
            submitSearch();
        });

        if (searchInput) {
            searchInput.addEventListener("input", () => {
                window.clearTimeout(timerId);
                timerId = window.setTimeout(submitSearch, debounceMs);
            });
        }

        filterSelects.forEach((select) => {
            select.addEventListener("change", submitSearch);
        });
    }

    const claimItemIdInput = document.getElementById("claimItemId");
    const claimModalImage = document.getElementById("claimModalImage");

    document.addEventListener("click", (event) => {
        const button = event.target.closest(".claim-btn");
        if (!button) {
            return;
        }

        if (claimItemIdInput) {
            claimItemIdInput.value = button.getAttribute("data-item-id") || "";
        }

        if (claimModalImage) {
            const imagePath = button.getAttribute("data-item-image");
            claimModalImage.src = imagePath || "";
            claimModalImage.style.display = imagePath ? "block" : "none";
        }
    });
});
