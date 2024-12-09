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
     * @param {string} setMeta
     * @return {HTMLElement}
     */
    function createDownloadSeriesButton(
        cloneFrom,
        setMeta) {
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
            console.log("Executing mass covering event! We will try to download the entirety of set " + setMeta)
            fetch("/JCoverXtreme/DownloadSeries",
                {
                    method: 'POST',
                    body: setMeta,
                    headers: {
                        "content-type": "application/json"
                    }
                }).then(console.log) // TODO: check out the root somehow. for now just assume /
        })
        return element
    }

    /**
     * Keep in sync with JCoverSharedController.URL_META_KEY
     * @type {string}
     */
    const URL_META_KEY = "JCoverXtremeProMeta"

    /**
     * Extract the JCoverXtremePro metadata from an image url.
     *
     * @param {string} url
     * @return {string}
     */
    function extractSetMeta(url) {
        return new URL(url).searchParams.get(URL_META_KEY)
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
            const cardContainer = findParent(element, ".cardBox")
            const cardImage = cardContainer.querySelector("a.cardImageContainer[href]")
            const setMeta = extractSetMeta(cardImage.href)
            if (downloadRowContainer.querySelector(`.${injectionMarker}`)) return
            // TODO: extract information about the series, and check if this is at all viable
            downloadRowContainer.appendChild(createDownloadSeriesButton(element, setMeta))
        })

    })
    observer.observe(document.body, {// TODO: selectively observe the body if at all possible
        subtree: true,
        childList: true,
    });
})()