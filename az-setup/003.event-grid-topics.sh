RG_FRIENDLY_NAME="organics"
APP_SHORT_URL="shorturl"
LOCATION="eastus2"
RESOURCE_GROUP_NAME="rg-$RG_FRIENDLY_NAME-openhack"

az eventgrid topic create --location $LOCATION \
                          --name "tenant-usage" \
                          --resource-group $RESOURCE_GROUP_NAME