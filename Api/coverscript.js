(function () {
    /**
     *
     * @param {HTMLElement} element
     * @param {string} selector
     * @returns {HTMLElement | null}
     */
    function findParent(element, selector) {
        let p = element
        while (p) {
            if (p.matches(selector)) return p
            p = p.parentNode
        }
        return null
    }

    const injectionMarker = "JCoverXtremePro-injection-marker";

    /**
     *
     * @param {HTMLElement} cloneFrom
     * @return {HTMLElement}
     */
    function createDownloadSeriesButton(cloneFrom) {
        /*<button is="paper-icon-button-light" class="btnDownloadRemoteImage autoSize paper-icon-button-light" raised"="" title="Download"><span class="material-icons cloud_download" aria-hidden="true"></span></button>*/
        //import LayersIcon from '@mui/icons-material/Layers';
        //import CloudDownloadIcon from '@mui/icons-material/CloudDownload';
        const element = document.createElement("button")
        element.classList.add(...cloneFrom.classList)
        element.classList.add(injectionMarker)
        element.title = "Download Series"
        const icon = document.createElement("span")
        icon.classList.add("material-icons", "burst_mode")
        icon.setAttribute("aria-hidden", "true")
        element.appendChild(icon)
        element.addEventListener("click", ev => {
            ev.preventDefault()
            
            alert("YOU HAVE JUST BEEN INTERDICTED BY THE JCOVERXTREMEPRO SERIES DOWNLOADIFICATOR")
        })
        return element
    }

    const observer = new MutationObserver(() => {
        console.log("JCoverXtremePro observation was triggered!")
        console.log("Listing all download buttons")
        /**
         * @type {NodeListOf<Element>}
         */
        const buttons = document.querySelectorAll(".imageEditorCard .cardFooter .btnDownloadRemoteImage")

        buttons.forEach(element => {
            const downloadRowContainer = findParent(element, ".cardText")
            if (downloadRowContainer.querySelector(`.${injectionMarker}`)) return
            // TODO: extract information about the series, and check if this is at all viable
            downloadRowContainer.appendChild(createDownloadSeriesButton(element))
        })

    })
    observer.observe(document.body, {// TODO: selectively observe the body if at all possible
        subtree: true,
        childList: true,
    });
})()