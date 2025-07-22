# GitHub Actions Self-Hosted Runner Container for ARM64
FROM ubuntu:22.04

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive
ENV RUNNER_ALLOW_RUNASROOT=1

# First install basic dependencies in smaller batches to avoid I/O issues
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    ca-certificates \
    gnupg \
    lsb-release \
    && rm -rf /var/lib/apt/lists/*

# Install development tools
RUN apt-get update && apt-get install -y \
    git \
    jq \
    build-essential \
    libssl-dev \
    libffi-dev \
    sudo \
    && rm -rf /var/lib/apt/lists/*

# Install programming languages
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    nodejs \
    npm \
    && rm -rf /var/lib/apt/lists/*

# Install Docker
RUN curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
RUN echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
RUN apt-get update && apt-get install -y docker-ce-cli && rm -rf /var/lib/apt/lists/*

# Install Docker Compose
RUN curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
RUN chmod +x /usr/local/bin/docker-compose

# Install .NET SDK with better error handling for ARM64
RUN ARCH=$(dpkg --print-architecture) && \
    echo "Detected architecture: $ARCH" && \
    if [ "$ARCH" = "arm64" ] || [ "$ARCH" = "aarch64" ]; then \
    echo "Installing .NET SDK for ARM64..." && \
    mkdir -p /opt/dotnet && \
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.306 --install-dir /opt/dotnet && \
    ln -sf /opt/dotnet/dotnet /usr/local/bin/dotnet && \
    chmod +x /usr/local/bin/dotnet && \
    echo "export PATH=\$PATH:/opt/dotnet" >> /etc/bash.bashrc; \
    else \
    echo "Installing .NET SDK for x64..." && \
    wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y dotnet-sdk-8.0 && \
    rm -f packages-microsoft-prod.deb && \
    rm -rf /var/lib/apt/lists/*; \
    fi

# Create runner user and docker group
RUN groupadd docker || true
RUN useradd -m -s /bin/bash runner && usermod -aG docker runner

# Switch to runner user and set up workspace
USER runner
WORKDIR /home/runner

# Download and install GitHub Actions runner with architecture detection
RUN ARCH=$(dpkg --print-architecture) && \
    echo "Detected architecture: $ARCH" && \
    if [ "$ARCH" = "arm64" ]; then \
    echo "Downloading ARM64 runner..." && \
    curl -o actions-runner-linux-arm64-2.325.0.tar.gz -L https://github.com/actions/runner/releases/download/v2.325.0/actions-runner-linux-arm64-2.325.0.tar.gz && \
    tar xzf ./actions-runner-linux-arm64-2.325.0.tar.gz && \
    rm actions-runner-linux-arm64-2.325.0.tar.gz; \
    else \
    echo "Downloading x64 runner..." && \
    curl -o actions-runner-linux-x64-2.325.0.tar.gz -L https://github.com/actions/runner/releases/download/v2.325.0/actions-runner-linux-x64-2.325.0.tar.gz && \
    tar xzf ./actions-runner-linux-x64-2.325.0.tar.gz && \
    rm actions-runner-linux-x64-2.325.0.tar.gz; \
    fi

# Install runner dependencies (switch back to root temporarily)
USER root
RUN /home/runner/bin/installdependencies.sh

# Copy entrypoint script
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

USER runner
WORKDIR /home/runner

ENTRYPOINT ["/entrypoint.sh"]
