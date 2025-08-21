#!/bin/bash

# Script to install FileProcessor as a Linux systemd service
# Run with sudo: sudo ./install-linux-service.sh

set -e

SERVICE_NAME="fileprocessor"
SERVICE_USER="fileprocessor"
SERVICE_GROUP="fileprocessor"
INSTALL_DIR="/opt/fileprocessor"
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

echo "Installing FileProcessor as a Linux systemd service..."

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "Error: This script must be run as root (use sudo)"
    exit 1
fi

# Create service user and group if they don't exist
if ! id "$SERVICE_USER" &>/dev/null; then
    echo "Creating service user: $SERVICE_USER"
    useradd --system --no-create-home --shell /bin/false "$SERVICE_USER"
else
    echo "Service user $SERVICE_USER already exists"
fi

# Create installation directory
echo "Creating installation directory: $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"
mkdir -p "$INSTALL_DIR/input"
mkdir -p "$INSTALL_DIR/output"
mkdir -p "$INSTALL_DIR/logs"

# Set ownership and permissions
chown -R "$SERVICE_USER:$SERVICE_GROUP" "$INSTALL_DIR"
chmod 755 "$INSTALL_DIR"
chmod 755 "$INSTALL_DIR/input"
chmod 755 "$INSTALL_DIR/output"
chmod 755 "$INSTALL_DIR/logs"

# Copy service files (assuming they're in the current directory)
if [ -f "FileProcessor.Service" ]; then
    echo "Copying service executable..."
    cp FileProcessor.Service "$INSTALL_DIR/"
    chmod +x "$INSTALL_DIR/FileProcessor.Service"
    chown "$SERVICE_USER:$SERVICE_GROUP" "$INSTALL_DIR/FileProcessor.Service"
else
    echo "Warning: FileProcessor.Service executable not found in current directory"
    echo "You'll need to copy it manually to $INSTALL_DIR/"
fi

# Copy configuration files
if [ -f "appsettings.json" ]; then
    cp appsettings.json "$INSTALL_DIR/"
    chown "$SERVICE_USER:$SERVICE_GROUP" "$INSTALL_DIR/appsettings.json"
fi

if [ -f "appsettings.Production.json" ]; then
    cp appsettings.Production.json "$INSTALL_DIR/"
    chown "$SERVICE_USER:$SERVICE_GROUP" "$INSTALL_DIR/appsettings.Production.json"
fi

# Install systemd service file
echo "Installing systemd service file..."
if [ -f "fileprocessor.service" ]; then
    cp fileprocessor.service "$SERVICE_FILE"
    
    # Update paths in service file
    sed -i "s|/opt/fileprocessor|$INSTALL_DIR|g" "$SERVICE_FILE"
    sed -i "s|User=fileprocessor|User=$SERVICE_USER|g" "$SERVICE_FILE"
    sed -i "s|Group=fileprocessor|Group=$SERVICE_GROUP|g" "$SERVICE_FILE"
else
    echo "Error: fileprocessor.service file not found"
    exit 1
fi

# Reload systemd and enable service
echo "Reloading systemd daemon..."
systemctl daemon-reload

echo "Enabling service..."
systemctl enable "$SERVICE_NAME"

echo "Starting service..."
systemctl start "$SERVICE_NAME"

# Check service status
echo "Service status:"
systemctl status "$SERVICE_NAME" --no-pager

echo ""
echo "Installation completed successfully!"
echo ""
echo "Service management commands:"
echo "  Start:   sudo systemctl start $SERVICE_NAME"
echo "  Stop:    sudo systemctl stop $SERVICE_NAME"
echo "  Restart: sudo systemctl restart $SERVICE_NAME"
echo "  Status:  sudo systemctl status $SERVICE_NAME"
echo "  Logs:    sudo journalctl -u $SERVICE_NAME -f"
echo "  Disable: sudo systemctl disable $SERVICE_NAME"
echo ""
echo "Configuration files are located in: $INSTALL_DIR"
echo "Log files will be written to: $INSTALL_DIR/logs"