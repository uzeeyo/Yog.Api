# Docker image
FROM ubuntu:latest
WORKDIR ./app
COPY . /app

# Setting environment variables
ARG ugs_project_id
ARG ugs_service_key_id
ARG ugs_service_secret_key
ENV UGS_CLI_PROJECT_ID=$ugs_project_id
ENV UGS_CLI_SERVICE_KEY_ID=$ugs_service_key_id
ENV UGS_CLI_SERVICE_SECRET_KEY=$ugs_service_secret_key
ENV UGS_CLI_ENVIRONMENT_NAME=production
# Update and get dependency packages
RUN apt-get update
RUN apt-get install sudo -y
RUN apt-get install gnupg -y
RUN apt-get update; apt-get install curl -y
RUN sudo apt-get install -y libicu-dev -y
RUN sudo apt-get install -y dotnet-sdk-8.0 -y
# Download and install UGS CLI from GitHub CLI here
RUN curl -sLo ugs_installer ugscli.unity.com/v1 && bash ugs_installer
# Run test commands on the CLI
RUN ugs --version
RUN ugs config get project-id
RUN ugs deploy ./ -j