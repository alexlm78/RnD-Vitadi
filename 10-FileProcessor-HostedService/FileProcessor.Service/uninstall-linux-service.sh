#!/bin/bash

# Script to uninstall FileProcessor Linux systemd service
# Run with sudo: sudo ./uninstall-linux-service.sh

set -e

SERVICE_NAME="fileprocessor"
SERVICE_USER="fileprocessor"
INSTALL_DIR="/opt/fileprocessor"
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

echo "Uninstalling FileProcessor systemd service..."

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "Error: This script must be run as root (use sudo)"
    exit 1
fi

# Stop and disable service if it exists
if systemctl is-active --quiet "$SERVICE_NAME"; then
    echo "Stopping service..."
    systemctl stop "$SERVICE_NAME"
fi

if systemctl is-enabled --quiet "$SERVICE_NAME"; then
    echo "Disabling service..."
    systemctl disable "$SERVICE_NAME"
fi

# Remove service file
if [ -f "$SERVICE_FILE" ]; then
    echo "Removing service file..."
    rm "$SERVICE_FILE"
    systemctl daemon-reload
else
    echo "Service file not found: $SERVICE_FILE"
fi

# Ask user if they want to remove installation directory
read -p "Do you want to remove the installation directory $INSTALL_DIR? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if [ -d "$INSTALL_DIR" ]; then
        echo "Removing installation directory..."
        rm -rf "$INSTALL_DIR"
        echo "Installation directory removed."
    else
        echo "Installation directory not found: $INSTALL_DIR"
    fi
else
    echo "Installation directory preserved: $INSTALL_DIR"
fi

# Ask user if they want to remove service user
read -p "Do you want to remove the service user $SERVICE_USER? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if id "$SERVICE_USER" &>/dev/null; then
        echo "Removing service user..."
        userdel "$SERVICE_USER"
        echo "Service user removed."
    else
        echo "Service user not found: $SERVICE_USER"
    fi
else
    echo "Service user preserved: $SERVICE_USER"
fi

echo ""
echo "Uninstallation completed successfully!"