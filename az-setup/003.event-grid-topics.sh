RG_FRIENDLY_NAME="organics"
APP_SHORT_URL="shorturl"
LOCATION="eastus2"
RESOURCE_GROUP_NAME="rg-$RG_FRIENDLY_NAME-openhack"
STORAGE_ACCOUNT_NAME="st"$APP_SHORT_URL"eventhub"
SKU="Standard_LRS"
KIND="StorageV2"
EVENTHUB_NAMESPACE="evhns-$APP_SHORT_URL"
EVENTHUB_NAME="evh-$APP_SHORT_URL"
QUEUE_NAME="stq-$APP_SHORT_URL-usage"
TOPIC_NAME="tenant-usage"

az eventgrid topic create --location $LOCATION \
                          --name $TOPIC_NAME \
                          --resource-group $RESOURCE_GROUP_NAME

storageid=$(az storage account show --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP_NAME --query id --output tsv)
echo "storageid: $storageid"

QUEUE_ID="$storageid/queueservices/default/queues/$QUEUE_NAME"
echo "queueid: $QUEUE_ID"

TOPIC_ID=$(az eventgrid topic show --name $TOPIC_NAME -g $RESOURCE_GROUP_NAME --query id --output tsv)
echo "topicid: $TOPIC_ID"

EVENT_SUBSCRIPTION_NAME="shorturl-usage"

az eventgrid event-subscription create \
  --source-resource-id $TOPIC_ID \
  --name $EVENT_SUBSCRIPTION_NAME \
  --endpoint-type storagequeue \
  --endpoint $QUEUE_ID \
  --expiration-date "2024-01-01"