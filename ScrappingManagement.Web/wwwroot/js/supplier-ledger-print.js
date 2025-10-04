$(document).ready(function () {
    // Initialize select2
    $('#selectedSupplierId').select2({
        theme: "bootstrap-5",
        placeholder: "-- Select Supplier --",
        width: 'resolve'
    });
   // $('#selectedSupplierId').next('.select2').find('.selection').addClass('form-control');

    // Clear button functionality
    $('a.btn-secondary').on('click', function () {
        $('#selectedSupplierId').val('').trigger('change');
        $('#fromDate').val('');
        $('#toDate').val('');
        $(this).closest('form').submit();
    });

    // Print functionality
    $('#printLedgerBtn').on('click', function () {
        var printContents = $('#ledgerContent').html();
        var selectedSupplierName = $('#selectedSupplierId option:selected').text().trim();

        if (!printContents || selectedSupplierName === "-- Select Supplier --") {
            alert("Please select a supplier and ensure ledger content is available before printing.");
            return;
        }

        // Create a hidden iframe for printing
        var printFrame = document.createElement('iframe');
        printFrame.style.position = 'absolute';
        printFrame.style.top = '-10000px';
        document.body.appendChild(printFrame);

        var doc = printFrame.contentWindow.document;

        var html = `
            <html>
                <head>
                    <title>Supplier Ledger</title>
                    <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                    <link rel="stylesheet" href="/css/site.css" asp-append-version="true" />
                    <link rel="stylesheet" href="/css/supplier-ledger-print.css" asp-append-version="true" />
                </head>
                <body>
                    <h4>Ledger for ${selectedSupplierName}</h4>
                    <div class="container-fluid">
                        ${printContents}
                    </div>
                </body>
            </html>
        `;

        doc.open();
        doc.write(html);
        doc.close();

        printFrame.onload = function () {
            printFrame.contentWindow.focus();
            printFrame.contentWindow.print();
            setTimeout(function () {
                document.body.removeChild(printFrame);
            }, 1000);
        };
    });
});
