from celery import Celery

app = Celery('inpainter', backend='rpc://', broker='amqp://user:password@broker:5672')


@app.task
def add(x, y):
    return x + y
