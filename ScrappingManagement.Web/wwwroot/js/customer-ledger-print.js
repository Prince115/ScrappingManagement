$(document).ready(function () {
    $('#selectedCustomerId').select2({
        theme: "bootstrap-5",
        placeholder: "-- Select Customer --",
        width: 'resolve'
    });

    // Clear button functionality
    $('a.btn-secondary').on('click', function () {
        $('#selectedCustomerId').val('').trigger('change');
        $('#fromDate').val('');
        $('#toDate').val('');
        $(this).closest('form').submit();
    });

    $('#printLedgerBtn').on('click', function () {
        var printContents = $('#ledgerContent').html();
        var selectedCustomerName = $('#selectedCustomerId option:selected').text().trim();

        if (!printContents || selectedCustomerName === "-- Select Customer --") {
            alert("Please select a Customer and ensure ledger content is available before printing.");
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
                    <title>Customer Ledger</title>
                    <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                    <link rel="stylesheet" href="/css/site.css" asp-append-version="true" />
                    <link rel="stylesheet" href="/css/Customer-ledger-print.css" asp-append-version="true" />
                </head>
                <body>
                    <h4>Ledger for ${selectedCustomerName}</h4>
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
