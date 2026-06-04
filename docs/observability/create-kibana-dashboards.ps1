param(
    [string]$KibanaUrl = "http://localhost:5601"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$headers = @{
    "kbn-xsrf" = "marketplace-observability"
}

$dataViewId = "marketplace-backend-logs"
$kibanaBaseUrl = $KibanaUrl.TrimEnd("/")

function ConvertTo-CompressedJson {
    param([Parameter(Mandatory = $true)] [object]$Value)

    return $Value | ConvertTo-Json -Depth 50 -Compress
}

function Invoke-KibanaSavedObjectUpsert {
    param(
        [Parameter(Mandatory = $true)] [string]$Type,
        [Parameter(Mandatory = $true)] [string]$Id,
        [Parameter(Mandatory = $true)] [hashtable]$Attributes,
        [array]$References = @()
    )

    $body = @{
        attributes = $Attributes
        references = $References
    } | ConvertTo-Json -Depth 60

    $uri = "{0}/api/saved_objects/{1}/{2}?overwrite=true" -f $kibanaBaseUrl, $Type, $Id
    Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -ContentType "application/json" -Body $body | Out-Null
}

function New-SearchSourceJson {
    param([string]$Query = "")

    return ConvertTo-CompressedJson @{
        query = @{
            query = $Query
            language = "kuery"
        }
        indexRefName = "kibanaSavedObjectMeta.searchSourceJSON.index"
        filter = @()
    }
}

function New-IndexReference {
    return @{
        name = "kibanaSavedObjectMeta.searchSourceJSON.index"
        type = "index-pattern"
        id = $dataViewId
    }
}

function New-VisualizationAttributes {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [string]$Query = "",
        [Parameter(Mandatory = $true)] [hashtable]$VisState
    )

    return @{
        title = $Title
        description = ""
        version = 1
        visState = ConvertTo-CompressedJson $VisState
        uiStateJSON = "{}"
        kibanaSavedObjectMeta = @{
            searchSourceJSON = New-SearchSourceJson $Query
        }
    }
}

function New-MetricVisState {
    param([Parameter(Mandatory = $true)] [string]$Title)

    return @{
        title = $Title
        type = "metric"
        aggs = @(
            @{
                id = "1"
                enabled = $true
                type = "count"
                schema = "metric"
                params = @{}
            }
        )
        params = @{
            type = "metric"
            addTooltip = $true
            addLegend = $false
            metric = @{
                metricColorMode = "None"
                colorSchema = "Green to Red"
                useRanges = $false
                invertColors = $false
                percentageMode = $false
                colorsRange = @(@{ from = 0; to = 10000 })
                labels = @{ show = $true }
                style = @{
                    bgFill = "#000"
                    bgColor = $false
                    labelColor = $false
                    fontSize = 48
                    subText = ""
                }
            }
        }
    }
}

function New-LineVisState {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [Parameter(Mandatory = $true)] [string]$SplitField
    )

    return @{
        title = $Title
        type = "line"
        aggs = @(
            @{
                id = "1"
                enabled = $true
                type = "count"
                schema = "metric"
                params = @{}
            },
            @{
                id = "2"
                enabled = $true
                type = "date_histogram"
                schema = "segment"
                params = @{
                    field = "@timestamp"
                    interval = "auto"
                    useNormalizedEsInterval = $true
                    scaleMetricValues = $false
                    drop_partials = $false
                    min_doc_count = 1
                    extended_bounds = @{}
                }
            },
            @{
                id = "3"
                enabled = $true
                type = "terms"
                schema = "group"
                params = @{
                    field = $SplitField
                    orderBy = "1"
                    order = "desc"
                    size = 5
                    otherBucket = $false
                    missingBucket = $false
                }
            }
        )
        params = @{
            type = "line"
            addTooltip = $true
            addLegend = $true
            legendPosition = "right"
            addTimeMarker = $false
            times = @()
            grid = @{ categoryLines = $false }
            categoryAxes = @(
                @{
                    id = "CategoryAxis-1"
                    type = "category"
                    position = "bottom"
                    show = $true
                    style = @{}
                    scale = @{ type = "linear" }
                    labels = @{ show = $true; truncate = 100 }
                    title = @{}
                }
            )
            valueAxes = @(
                @{
                    id = "ValueAxis-1"
                    name = "LeftAxis-1"
                    type = "value"
                    position = "left"
                    show = $true
                    style = @{}
                    scale = @{ type = "linear"; mode = "normal" }
                    labels = @{ show = $true; rotate = 0; filter = $false; truncate = 100 }
                    title = @{ text = "Count" }
                }
            )
            seriesParams = @(
                @{
                    show = $true
                    type = "line"
                    mode = "normal"
                    valueAxis = "ValueAxis-1"
                    drawLinesBetweenPoints = $true
                    showCircles = $true
                    data = @{ id = "1"; label = "Count" }
                }
            )
        }
    }
}

function New-HorizontalBarVisState {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [Parameter(Mandatory = $true)] [string]$BucketField,
        [string]$MetricType = "count",
        [string]$MetricField = "",
        [string]$AxisTitle = "Count"
    )

    $metricParams = @{}
    if ($MetricField.Length -gt 0) {
        $metricParams.field = $MetricField
    }

    return @{
        title = $Title
        type = "horizontal_bar"
        aggs = @(
            @{
                id = "1"
                enabled = $true
                type = $MetricType
                schema = "metric"
                params = $metricParams
            },
            @{
                id = "2"
                enabled = $true
                type = "terms"
                schema = "segment"
                params = @{
                    field = $BucketField
                    orderBy = "1"
                    order = "desc"
                    size = 10
                    otherBucket = $false
                    missingBucket = $false
                }
            }
        )
        params = @{
            type = "histogram"
            addTooltip = $true
            addLegend = $false
            legendPosition = "right"
            times = @()
            grid = @{ categoryLines = $false }
            categoryAxes = @(
                @{
                    id = "CategoryAxis-1"
                    type = "category"
                    position = "left"
                    show = $true
                    style = @{}
                    scale = @{ type = "linear" }
                    labels = @{ show = $true; truncate = 100 }
                    title = @{}
                }
            )
            valueAxes = @(
                @{
                    id = "ValueAxis-1"
                    name = "LeftAxis-1"
                    type = "value"
                    position = "bottom"
                    show = $true
                    style = @{}
                    scale = @{ type = "linear"; mode = "normal" }
                    labels = @{ show = $true; rotate = 0; filter = $false; truncate = 100 }
                    title = @{ text = $AxisTitle }
                }
            )
            seriesParams = @(
                @{
                    show = $true
                    type = "histogram"
                    mode = "normal"
                    valueAxis = "ValueAxis-1"
                    data = @{ id = "1"; label = $AxisTitle }
                }
            )
        }
    }
}

function New-TableVisState {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [Parameter(Mandatory = $true)] [string[]]$BucketFields
    )

    $aggs = @(
        @{
            id = "1"
            enabled = $true
            type = "count"
            schema = "metric"
            params = @{}
        }
    )

    $bucketId = 2
    foreach ($bucketField in $BucketFields) {
        $aggs += @{
            id = "$bucketId"
            enabled = $true
            type = "terms"
            schema = "bucket"
            params = @{
                field = $bucketField
                orderBy = "1"
                order = "desc"
                size = 10
                otherBucket = $false
                missingBucket = $false
            }
        }
        $bucketId += 1
    }

    return @{
        title = $Title
        type = "table"
        aggs = $aggs
        params = @{
            perPage = 10
            showPartialRows = $false
            showMetricsAtAllLevels = $false
            showTotal = $false
            totalFunc = "sum"
        }
    }
}

function New-EndpointTableVisState {
    param([Parameter(Mandatory = $true)] [string]$Title)

    return @{
        title = $Title
        type = "table"
        aggs = @(
            @{
                id = "1"
                enabled = $true
                type = "count"
                schema = "metric"
                params = @{}
            },
            @{
                id = "2"
                enabled = $true
                type = "terms"
                schema = "bucket"
                params = @{
                    field = "url.path"
                    orderBy = "1"
                    order = "desc"
                    size = 10
                    otherBucket = $false
                    missingBucket = $false
                }
            }
        )
        params = @{
            perPage = 10
            showPartialRows = $false
            showMetricsAtAllLevels = $false
            showTotal = $false
            totalFunc = "sum"
        }
    }
}

function New-EndpointDurationTableVisState {
    param([Parameter(Mandatory = $true)] [string]$Title)

    return @{
        title = $Title
        type = "table"
        aggs = @(
            @{
                id = "1"
                enabled = $true
                type = "avg"
                schema = "metric"
                params = @{ field = "metadata.DurationMs" }
            },
            @{
                id = "2"
                enabled = $true
                type = "terms"
                schema = "bucket"
                params = @{
                    field = "url.path"
                    orderBy = "1"
                    order = "desc"
                    size = 10
                    otherBucket = $false
                    missingBucket = $false
                }
            }
        )
        params = @{
            perPage = 10
            showPartialRows = $false
            showMetricsAtAllLevels = $false
            showTotal = $false
            totalFunc = "sum"
        }
    }
}

function New-DashboardPanel {
    param(
        [Parameter(Mandatory = $true)] [string]$PanelId,
        [Parameter(Mandatory = $true)] [string]$ReferenceName,
        [Parameter(Mandatory = $true)] [int]$X,
        [Parameter(Mandatory = $true)] [int]$Y,
        [Parameter(Mandatory = $true)] [int]$Width,
        [Parameter(Mandatory = $true)] [int]$Height,
        [string]$Type = "visualization"
    )

    return @{
        version = "8.15.3"
        panelIndex = $ReferenceName
        panelRefName = $ReferenceName
        type = $Type
        gridData = @{
            i = $ReferenceName
            x = $X
            y = $Y
            w = $Width
            h = $Height
        }
        embeddableConfig = @{}
    }
}

function New-DashboardReference {
    param(
        [Parameter(Mandatory = $true)] [string]$ReferenceName,
        [Parameter(Mandatory = $true)] [string]$SavedObjectId,
        [string]$Type = "visualization"
    )

    return @{
        name = $ReferenceName
        type = $Type
        id = $SavedObjectId
    }
}

function New-SavedSearchAttributes {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [Parameter(Mandatory = $true)] [string]$Query,
        [Parameter(Mandatory = $true)] [string[]]$Columns,
        [string]$SortField = "@timestamp",
        [string]$SortDirection = "desc"
    )

    return @{
        title = $Title
        description = ""
        hits = 0
        columns = $Columns
        sort = @(@($SortField, $SortDirection))
        kibanaSavedObjectMeta = @{
            searchSourceJSON = New-SearchSourceJson $Query
        }
    }
}

function New-DashboardAttributes {
    param(
        [Parameter(Mandatory = $true)] [string]$Title,
        [Parameter(Mandatory = $true)] [string]$Description,
        [Parameter(Mandatory = $true)] [array]$Panels
    )

    return @{
        title = $Title
        description = $Description
        version = 1
        hits = 0
        timeRestore = $true
        timeFrom = "now-24h"
        timeTo = "now"
        refreshInterval = @{
            pause = $true
            value = 60000
        }
        optionsJSON = ConvertTo-CompressedJson @{
            useMargins = $true
            syncColors = $false
            hidePanelTitles = $false
            syncCursor = $true
            syncTooltips = $true
        }
        panelsJSON = ConvertTo-CompressedJson $Panels
        kibanaSavedObjectMeta = @{
            searchSourceJSON = ConvertTo-CompressedJson @{
                query = @{ query = ""; language = "kuery" }
                filter = @()
            }
        }
    }
}

Invoke-KibanaSavedObjectUpsert -Type "index-pattern" -Id $dataViewId -Attributes @{
    title = "logs-marketplace-backend-*"
    name = "Marketplace Backend Logs"
    timeFieldName = "@timestamp"
    fields = "[]"
    fieldAttrs = "{}"
    fieldFormatMap = "{}"
    runtimeFieldMap = "{}"
    sourceFilters = "[]"
    allowHidden = $false
}

$visualizations = @(
    @{
        Id = "vis-system-requests-total"
        Title = "System: Total Requests"
        Query = 'labels.Component: "HttpPipeline"'
        State = New-MetricVisState "System: Total Requests"
    },
    @{
        Id = "vis-system-errors-total"
        Title = "System: Failed Requests"
        Query = 'labels.Component: "HttpPipeline" and labels.Outcome: "Failed"'
        State = New-MetricVisState "System: Failed Requests"
    },
    @{
        Id = "vis-system-requests-over-time"
        Title = "System: Requests Over Time by Status Code"
        Query = 'labels.Component: "HttpPipeline"'
        State = New-LineVisState "System: Requests Over Time by Status Code" "http.response.status_code"
    },
    @{
        Id = "vis-system-top-endpoints"
        Title = "System: Top Endpoints"
        Query = 'labels.Component: "HttpPipeline"'
        State = New-EndpointTableVisState "System: Top Endpoints"
    },
    @{
        Id = "vis-system-duration-by-endpoint"
        Title = "System: Average Request Duration by Endpoint (ms)"
        Query = 'labels.Component: "HttpPipeline" and metadata.DurationMs: *'
        State = New-EndpointDurationTableVisState "System: Average Request Duration by Endpoint (ms)"
    },
    @{
        Id = "vis-system-log-levels"
        Title = "System: Logs by Level"
        Query = ""
        State = New-HorizontalBarVisState "System: Logs by Level" "log.level"
    },
    @{
        Id = "vis-checkout-completed"
        Title = "Checkout: Completed Count"
        Query = '(labels.Operation: "Checkout.Process" and labels.Outcome: "Succeeded") or labels.Operation: "Checkout.Completed"'
        State = New-MetricVisState "Checkout: Completed Count"
    },
    @{
        Id = "vis-checkout-failed"
        Title = "Checkout: Failed Count"
        Query = '(labels.Operation: "Checkout.Process" and labels.Outcome: "Failed") or labels.Operation: "Checkout.Failed"'
        State = New-MetricVisState "Checkout: Failed Count"
    },
    @{
        Id = "vis-checkout-operations-over-time"
        Title = "Checkout: Operations Over Time"
        Query = 'labels.Component: "CheckoutService"'
        State = New-LineVisState "Checkout: Operations Over Time" "labels.Operation"
    },
    @{
        Id = "vis-checkout-duration-by-operation"
        Title = "Checkout: Average Duration by Operation"
        Query = 'labels.Component: "CheckoutService" and metadata.DurationMs: *'
        State = New-HorizontalBarVisState "Checkout: Average Duration by Operation" "labels.Operation" "avg" "metadata.DurationMs" "Average ms"
    },
    @{
        Id = "vis-checkout-failures-by-type"
        Title = "Checkout: Failures by Error Type"
        Query = '(labels.Operation: "Checkout.Process" and labels.Outcome: "Failed") or labels.Operation: "Checkout.Failed"'
        State = New-HorizontalBarVisState "Checkout: Failures by Error Type" "error.type"
    },
    @{
        Id = "vis-checkout-debug-table"
        Title = "Checkout: Recent Events"
        Query = 'labels.Component: "CheckoutService"'
        State = New-TableVisState "Checkout: Recent Events" @("labels.CorrelationId", "labels.Operation", "labels.Outcome")
    }
)

foreach ($visualization in $visualizations) {
    Invoke-KibanaSavedObjectUpsert `
        -Type "visualization" `
        -Id $visualization.Id `
        -Attributes (New-VisualizationAttributes $visualization.Title $visualization.Query $visualization.State) `
        -References @(New-IndexReference)
}

$checkoutDetailColumns = @(
    "@timestamp",
    "labels.CorrelationId",
    "labels.Operation",
    "labels.Outcome",
    "metadata.DurationMs",
    "error.type",
    "metadata.CartId",
    "metadata.OrderId",
    "labels.OrderNumber",
    "metadata.UserId"
)

$savedSearches = @(
    @{
        Id = "search-checkout-recent-operation-events"
        Title = "Checkout: Recent Operation Events"
        Query = 'labels.Component: "CheckoutService"'
        Columns = $checkoutDetailColumns
        SortField = "@timestamp"
        SortDirection = "desc"
    },
    @{
        Id = "search-checkout-slowest-operation-events"
        Title = "Checkout: Slowest Operation Events"
        Query = 'labels.Component: "CheckoutService" and metadata.DurationMs: *'
        Columns = $checkoutDetailColumns
        SortField = "metadata.DurationMs"
        SortDirection = "desc"
    },
    @{
        Id = "search-checkout-failed-examples"
        Title = "Checkout: Failed Checkout Examples"
        Query = '(labels.Operation: "Checkout.Process" and labels.Outcome: "Failed") or labels.Operation: "Checkout.Failed"'
        Columns = $checkoutDetailColumns
        SortField = "@timestamp"
        SortDirection = "desc"
    }
)

foreach ($savedSearch in $savedSearches) {
    Invoke-KibanaSavedObjectUpsert `
        -Type "search" `
        -Id $savedSearch.Id `
        -Attributes (New-SavedSearchAttributes $savedSearch.Title $savedSearch.Query $savedSearch.Columns $savedSearch.SortField $savedSearch.SortDirection) `
        -References @(New-IndexReference)
}

$systemPanels = @(
    New-DashboardPanel "vis-system-requests-total" "panel_system_requests_total" 0 0 12 8
    New-DashboardPanel "vis-system-errors-total" "panel_system_errors_total" 12 0 12 8
    New-DashboardPanel "vis-system-requests-over-time" "panel_system_requests_over_time" 0 8 24 12
    New-DashboardPanel "vis-system-top-endpoints" "panel_system_top_endpoints" 0 20 12 12
    New-DashboardPanel "vis-system-duration-by-endpoint" "panel_system_duration_by_endpoint" 12 20 12 12
    New-DashboardPanel "vis-system-log-levels" "panel_system_log_levels" 0 32 24 10
)

$systemReferences = @(
    New-DashboardReference "panel_system_requests_total" "vis-system-requests-total"
    New-DashboardReference "panel_system_errors_total" "vis-system-errors-total"
    New-DashboardReference "panel_system_requests_over_time" "vis-system-requests-over-time"
    New-DashboardReference "panel_system_top_endpoints" "vis-system-top-endpoints"
    New-DashboardReference "panel_system_duration_by_endpoint" "vis-system-duration-by-endpoint"
    New-DashboardReference "panel_system_log_levels" "vis-system-log-levels"
)

Invoke-KibanaSavedObjectUpsert `
    -Type "dashboard" `
    -Id "dashboard-marketplace-system-overview" `
    -Attributes (New-DashboardAttributes "Marketplace System Overview" "Request volume, status codes, endpoint traffic, request duration, and log levels from backend logs." $systemPanels) `
    -References $systemReferences

$checkoutPanels = @(
    New-DashboardPanel "vis-checkout-completed" "panel_checkout_completed" 0 0 12 8
    New-DashboardPanel "vis-checkout-failed" "panel_checkout_failed" 12 0 12 8
    New-DashboardPanel "vis-checkout-operations-over-time" "panel_checkout_operations_over_time" 0 8 24 12
    New-DashboardPanel "vis-checkout-duration-by-operation" "panel_checkout_duration_by_operation" 0 20 12 12
    New-DashboardPanel "vis-checkout-failures-by-type" "panel_checkout_failures_by_type" 12 20 12 12
    New-DashboardPanel "vis-checkout-debug-table" "panel_checkout_debug_table" 0 32 24 12
    New-DashboardPanel "search-checkout-recent-operation-events" "panel_checkout_recent_operation_events" 0 44 24 12 "search"
    New-DashboardPanel "search-checkout-slowest-operation-events" "panel_checkout_slowest_operation_events" 0 56 24 12 "search"
    New-DashboardPanel "search-checkout-failed-examples" "panel_checkout_failed_examples" 0 68 24 12 "search"
)

$checkoutReferences = @(
    New-DashboardReference "panel_checkout_completed" "vis-checkout-completed"
    New-DashboardReference "panel_checkout_failed" "vis-checkout-failed"
    New-DashboardReference "panel_checkout_operations_over_time" "vis-checkout-operations-over-time"
    New-DashboardReference "panel_checkout_duration_by_operation" "vis-checkout-duration-by-operation"
    New-DashboardReference "panel_checkout_failures_by_type" "vis-checkout-failures-by-type"
    New-DashboardReference "panel_checkout_debug_table" "vis-checkout-debug-table"
    New-DashboardReference "panel_checkout_recent_operation_events" "search-checkout-recent-operation-events" "search"
    New-DashboardReference "panel_checkout_slowest_operation_events" "search-checkout-slowest-operation-events" "search"
    New-DashboardReference "panel_checkout_failed_examples" "search-checkout-failed-examples" "search"
)

Invoke-KibanaSavedObjectUpsert `
    -Type "dashboard" `
    -Id "dashboard-marketplace-checkout-observability" `
    -Attributes (New-DashboardAttributes "Marketplace Checkout Observability" "Checkout operation counts, failures, duration by operation, and correlation-oriented debugging panels." $checkoutPanels) `
    -References $checkoutReferences

Write-Host "Created or updated Kibana observability dashboards at $kibanaBaseUrl."
Write-Host "System overview: $kibanaBaseUrl/app/dashboards#/view/dashboard-marketplace-system-overview"
Write-Host "Checkout observability: $kibanaBaseUrl/app/dashboards#/view/dashboard-marketplace-checkout-observability"
