# GitHub Actions Deployment Setup

This document explains how to set up and use the GitHub Actions deployment workflow for the Takhlees Tech Backend microservices.

## Overview

The deployment workflow automatically:
- Deploys to your server via SSH when code is pushed to `main`, `master`, or `stable-performance` branches
- Stops existing containers, builds new ones, and starts them
- Sends detailed email notifications about deployment status
- Provides comprehensive deployment information including service URLs and statistics

## Required GitHub Secrets

You need to configure the following secrets in your GitHub repository settings:

### Server Connection
- `SERVER_HOST`: Your server's IP address or hostname
- `SERVER_USERNAME`: SSH username for server access
- `SERVER_SSH_PRIVATE_KEY`: Private SSH key for server authentication
- `SERVER_PORT`: SSH port (usually 22)
- `SERVER_PROJECT_PATH`: Full path to your project directory on the server

### Email Notifications
- `SMTP_SERVER`: SMTP server address (e.g., smtp.gmail.com)
- `SMTP_PORT`: SMTP port (usually 587 for TLS)
- `SMTP_USERNAME`: SMTP username/email
- `SMTP_PASSWORD`: SMTP password or app-specific password
- `SMTP_FROM_EMAIL`: Email address to send notifications from

## How to Set Up GitHub Secrets

1. Go to your GitHub repository
2. Click on **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret with the exact names listed above

## Deployment Triggers

The workflow runs automatically when:
- Code is pushed to `main`, `master`, or `stable-performance` branches
- Manual deployment is triggered via GitHub Actions UI

## Manual Deployment

1. Go to your repository on GitHub
2. Click on **Actions** tab
3. Select **Deploy to Server** workflow
4. Click **Run workflow**
5. Choose environment (production or staging)
6. Click **Run workflow**

## Services Deployed

The workflow deploys the following microservices:

| Service | Subdomain | Description |
|---------|-----------|-------------|
| Admin Service | admin.takhleesak.com | Administrative panel and logging |
| Customer Service | customerservices.takhleesak.com | Customer service management |
| Auth Service | firstproject.takhleesak.com | Authentication and authorization |
| User Service | user.takhleesak.com | User management and orders |
| Nginx Proxy | Port 8090 | Reverse proxy and load balancer |
| phpMyAdmin | Port 8080 | Database management interface |

## Environment URLs

The workflow automatically generates URLs for different environments:

### Production (main/stable-performance branches)
- Main App: https://test.takhleesak.com/
- Admin Service: https://admin.takhleesak.com/swagger/index.html
- Customer Service: https://customerservices.takhleesak.com/swagger/index.html
- Auth Service: https://firstproject.takhleesak.com/swagger/index.html
- User Service: https://user.takhleesak.com/swagger/index.html
- phpMyAdmin: https://takhleesak.com:8080/

### Staging (when manually selected)
- Main App: https://staging.takhleesak.com/
- Admin Service: https://staging-admin.takhleesak.com/swagger/index.html
- Customer Service: https://staging-customerservices.takhleesak.com/swagger/index.html
- Auth Service: https://staging-firstproject.takhleesak.com/swagger/index.html
- User Service: https://staging-user.takhleesak.com/swagger/index.html
- phpMyAdmin: https://staging.takhleesak.com:8080/

## Email Notifications

The workflow sends detailed email notifications that include:
- Deployment status (success/failure)
- Environment and branch information
- Commit details and statistics
- Service URLs and direct links to Swagger API documentation
- Beautiful HTML formatting with service cards

## API Documentation

Each service provides Swagger API documentation accessible at:
- [Admin Service API](https://admin.takhleesak.com/swagger/index.html)
- [Customer Service API](https://customerservices.takhleesak.com/swagger/index.html)
- [Auth Service API](https://firstproject.takhleesak.com/swagger/index.html)
- [User Service API](https://user.takhleesak.com/swagger/index.html)

## Troubleshooting

### Common Issues

1. **SSH Connection Failed**
   - Verify `SERVER_HOST`, `SERVER_USERNAME`, and `SERVER_PORT`
   - Ensure SSH key is correctly formatted (include `-----BEGIN` and `-----END` lines)
   - Check that the SSH key has proper permissions on the server

2. **Docker Commands Failed**
   - Ensure Docker and Docker Compose are installed on the server
   - Verify the user has permission to run Docker commands
   - Check if the `SERVER_PROJECT_PATH` is correct

3. **Email Notifications Not Sent**
   - Verify all SMTP settings are correct
   - For Gmail, use an App Password instead of your regular password
   - Check spam folder for notification emails

4. **Service URLs Not Accessible**
   - Verify DNS configuration for subdomains
   - Check nginx configuration for proxy rules
   - Ensure SSL certificates are properly configured

### Deployment Logs

To view deployment logs:
1. Go to **Actions** tab in your repository
2. Click on the deployment workflow run
3. Expand the steps to view detailed logs

## Customization

### Changing Environment URLs
Edit the `Get deployment info` step in `.github/workflows/deploy.yml` to update URLs for your specific domains.

### Adding New Services
If you add new services to your `docker-compose.yml`, update:
1. The service health check in the deployment script
2. The service cards in the email notification template
3. The deployment summary output

### Modifying Email Recipients
Change the `to:` field in the email notification step to send to different recipients.

## Security Best Practices

1. **Never commit secrets to your repository**
2. **Use environment-specific secrets** for different deployment stages
3. **Regularly rotate SSH keys and passwords**
4. **Monitor deployment logs** for suspicious activity
5. **Use strong passwords** for all services
6. **Keep API documentation secure** if it contains sensitive information

## Support

For issues with this deployment workflow, check:
1. GitHub Actions logs for specific error messages
2. Server logs for Docker-related issues
3. Email server logs for notification problems
4. DNS and SSL certificate configuration for subdomain issues

Remember to customize the URLs and paths according to your specific server setup and domain configuration. 