#!/bin/bash

# ================================================
# Docker Local Testing Script
# ================================================
# This script builds and runs the Docker container
# locally for testing before deploying to CapRover
# ================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
IMAGE_NAME="vorluno-contacto"
CONTAINER_NAME="contacto-test"
PORT=8080

# Check if .env file exists
if [ ! -f .env ]; then
    echo -e "${YELLOW}Warning: .env file not found${NC}"
    echo "Creating .env from .env.example..."
    cp .env.example .env
    echo -e "${RED}Please edit .env with your actual values and run this script again${NC}"
    exit 1
fi

# Load environment variables
source .env

# Validate required variables
REQUIRED_VARS=(
    "Email__Brevo__ApiKey"
    "Email__From"
    "Email__To"
    "Email__Logo__Url"
)

for var in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!var}" ]; then
        echo -e "${RED}Error: Required variable $var is not set in .env${NC}"
        exit 1
    fi
done

echo -e "${GREEN}✓ Environment variables loaded${NC}"

# Clean up previous container if exists
echo "Cleaning up previous container..."
docker stop $CONTAINER_NAME 2>/dev/null || true
docker rm $CONTAINER_NAME 2>/dev/null || true

# Build Docker image
echo -e "${YELLOW}Building Docker image...${NC}"
docker build -t $IMAGE_NAME:test .

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Docker build failed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker image built successfully${NC}"

# Run container
echo -e "${YELLOW}Starting container...${NC}"
docker run -d \
  --name $CONTAINER_NAME \
  -p $PORT:8080 \
  -e Email__Brevo__ApiKey="$Email__Brevo__ApiKey" \
  -e Email__From="$Email__From" \
  -e Email__To="$Email__To" \
  -e Email__Logo__Url="$Email__Logo__Url" \
  -e Email__Ack__From="${Email__Ack__From:-$Email__From}" \
  -e Email__Ack__Hero__Url="${Email__Ack__Hero__Url}" \
  -e GoogleSheets__WebhookUrl="${GoogleSheets__WebhookUrl}" \
  -e GoogleSheets__Token="${GoogleSheets__Token}" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  $IMAGE_NAME:test

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to start container${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Container started${NC}"

# Wait for container to be ready
echo "Waiting for container to be ready..."
sleep 5

# Check health
echo "Checking health endpoint..."
HEALTH_CHECK=$(curl -s http://localhost:$PORT/healthz || echo "failed")

if [ "$HEALTH_CHECK" = "Healthy" ]; then
    echo -e "${GREEN}✓ Health check passed${NC}"
else
    echo -e "${RED}✗ Health check failed${NC}"
    echo "Response: $HEALTH_CHECK"
    docker logs $CONTAINER_NAME
    exit 1
fi

# Show logs
echo -e "\n${YELLOW}Container logs:${NC}"
docker logs $CONTAINER_NAME

# Success message
echo -e "\n${GREEN}================================================${NC}"
echo -e "${GREEN}✓ Container is running successfully!${NC}"
echo -e "${GREEN}================================================${NC}"
echo -e "\nAccess the application at: ${GREEN}http://localhost:$PORT${NC}"
echo -e "\nUseful commands:"
echo -e "  View logs:    ${YELLOW}docker logs -f $CONTAINER_NAME${NC}"
echo -e "  Stop:         ${YELLOW}docker stop $CONTAINER_NAME${NC}"
echo -e "  Remove:       ${YELLOW}docker rm $CONTAINER_NAME${NC}"
echo -e "  Shell:        ${YELLOW}docker exec -it $CONTAINER_NAME sh${NC}"
echo -e "\nTo stop and clean up:"
echo -e "  ${YELLOW}docker stop $CONTAINER_NAME && docker rm $CONTAINER_NAME${NC}"
echo -e "\n"
